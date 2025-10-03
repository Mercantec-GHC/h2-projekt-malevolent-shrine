
using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleaningTasksController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CleaningTasksController> _logger;

        public CleaningTasksController(AppDBContext context, ILogger<CleaningTasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Manager/Admin/InfiniteVoid can create and assign tasks to cleaners
        [Authorize(Roles = RoleNames.Manager + "," + RoleNames.Admin + "," + RoleNames.InfiniteVoid)]
        [HttpPost]
        public async Task<ActionResult<CleaningTaskReadDto>> Create([FromBody] CleaningTaskCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate assigned user exists and has Cleaner (Rengøring) role
            var assigned = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == dto.AssignedToUserId);
            if (assigned == null)
                return BadRequest("Назначаемый пользователь не найден.");
            if (!string.Equals(assigned.Role.Name, RoleNames.Rengøring, StringComparison.Ordinal))
                return BadRequest("Назначать задачи можно только пользователям с ролью Rengøring (уборщик).");

            // Validate room if provided
            if (dto.RoomId.HasValue)
            {
                var roomExists = await _context.Rooms.AnyAsync(r => r.Id == dto.RoomId.Value);
                if (!roomExists)
                    return BadRequest($"Комната с ID {dto.RoomId.Value} не существует.");
            }

            // CreatedBy = current user id
            var creatorIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(creatorIdStr) || !int.TryParse(creatorIdStr, out var creatorId))
                return Unauthorized("Не удалось определить текущего пользователя.");

            var task = new CleaningTask
            {
                Title = dto.Title,
                Description = dto.Description,
                RoomId = dto.RoomId,
                AssignedToUserId = dto.AssignedToUserId,
                CreatedByUserId = creatorId,
                DueDate = dto.DueDate,
                Status = CleaningTaskStatus.New
            };

            _context.CleaningTasks.Add(task);
            await _context.SaveChangesAsync();

            var read = new CleaningTaskReadDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                RoomId = task.RoomId,
                AssignedToUserId = task.AssignedToUserId,
                CreatedByUserId = task.CreatedByUserId,
                DueDate = task.DueDate,
                Status = task.Status,
                CreatedAt = task.CreatedAt
            };
            return CreatedAtAction(nameof(GetMyTasks), new { }, read);
        }

        // Cleaners can list only their own tasks
        [Authorize(Roles = RoleNames.Rengøring)]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<CleaningTaskReadDto>>> GetMyTasks()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var tasks = await _context.CleaningTasks
                .Where(t => t.AssignedToUserId == userId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            var list = tasks.Select(t => new CleaningTaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                RoomId = t.RoomId,
                AssignedToUserId = t.AssignedToUserId,
                CreatedByUserId = t.CreatedByUserId,
                DueDate = t.DueDate,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(list);
        }

        // Cleaners can update status of their own task
        [Authorize(Roles = RoleNames.Rengøring)]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateMyTaskStatus(int id, [FromBody] CleaningTaskStatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var task = await _context.CleaningTasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
                return NotFound();

            if (task.AssignedToUserId != userId)
                return Forbid();

            task.Status = dto.Status;
            task.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Managers may view tasks assigned to a specific cleaner (optional helper)
        [Authorize(Roles = RoleNames.Manager + "," + RoleNames.Admin + "," + RoleNames.InfiniteVoid)]
        [HttpGet("assigned/{userId}")]
        public async Task<ActionResult<IEnumerable<CleaningTaskReadDto>>> GetTasksForUser(int userId)
        {
            var tasks = await _context.CleaningTasks
                .Where(t => t.AssignedToUserId == userId)
                .OrderBy(t => t.Status)
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            var list = tasks.Select(t => new CleaningTaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                RoomId = t.RoomId,
                AssignedToUserId = t.AssignedToUserId,
                CreatedByUserId = t.CreatedByUserId,
                DueDate = t.DueDate,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(list);
        }
    }
}
