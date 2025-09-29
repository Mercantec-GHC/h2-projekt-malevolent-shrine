using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    /// <summary>
    /// Endpoints for creating and managing support tickets and their messages.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDBContext _db;
        private readonly TicketRoutingService _routing;
        private readonly IHubContext<TicketHub> _hub;
        private readonly TelegramNotifier _notifier;

        /// <summary>
        /// Initializes a new instance of <see cref="TicketsController"/>.
        /// </summary>
        public TicketsController(AppDBContext db, TicketRoutingService routing, IHubContext<TicketHub> hub, TelegramNotifier notifier)
        {
            _db = db;
            _routing = routing;
            _hub = hub;
            _notifier = notifier;
        }

        /// <summary>
        /// Creates a new support ticket and posts the first message.
        /// </summary>
        /// <param name="dto">Ticket creation payload.</param>
        /// <returns>The created ticket.</returns>
        [HttpPost]
        public async Task<ActionResult<TicketReadDto>> Create([FromBody] TicketCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();

            var targetRole = _routing.ResolveTargetRoleName(dto.Category);

            var ticket = new Ticket
            {
                Title = dto.Title,
                Category = dto.Category,
                Status = TicketStatus.Open,
                CreatedByUserId = userId,
                BookingId = dto.BookingId,
                RoomId = dto.RoomId,
                TargetRoleName = targetRole,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            var firstMessage = new TicketMessage
            {
                TicketId = ticket.Id,
                SenderUserId = userId,
                Content = dto.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.TicketMessages.Add(firstMessage);
            await _db.SaveChangesAsync();

            var read = ToReadDto(ticket);
            // Сообщаем всем сотрудникам нужной роли о новом тикете
            await _hub.Clients.Group($"role-{targetRole}").SendAsync("TicketCreated", read);
            // Тост для админов/менеджеров
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "New ticket", Message = $"#{ticket.Id} {ticket.Title} → {targetRole}", Level = "info", Ts = DateTime.UtcNow });
            // Telegram уведомление (если настроено)
            await _notifier.SendAsync($"🆕 Ticket #{ticket.Id} — <b>{ticket.Title}</b> ({ticket.Category}) for role <b>{targetRole}</b>");

            return Ok(read);
        }

        /// <summary>
        /// Returns tickets created by the current user.
        /// </summary>
        [HttpGet("mine")]
        public async Task<ActionResult<List<TicketReadDto>>> GetMy()
        {
            var userId = GetUserId();
            var items = await _db.Tickets
                .Where(t => t.CreatedByUserId == userId)
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
            return Ok(items.Select(ToReadDto).ToList());
        }

        /// <summary>
        /// Returns tickets for the current user's role (admins see all).
        /// </summary>
        [HttpGet("for-role")]
        public async Task<ActionResult<List<TicketReadDto>>> GetForRole()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            {
                var all = await _db.Tickets.OrderByDescending(t => t.UpdatedAt).ToListAsync();
                return Ok(all.Select(ToReadDto).ToList());
            }
            if (string.IsNullOrEmpty(role)) return Forbid();

            var byRole = await _db.Tickets
                .Where(t => t.TargetRoleName == role)
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
            return Ok(byRole.Select(ToReadDto).ToList());
        }

        /// <summary>
        /// Returns ticket details with up to the last 200 messages.
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        /// <returns>Ticket and messages if authorized; 404 if not found.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOne(int id)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();
            if (!HasAccess(ticket)) return Forbid();

            var messages = await _db.TicketMessages
                .Where(m => m.TicketId == id)
                .OrderBy(m => m.CreatedAt)
                .Take(200)
                .ToListAsync();

            return Ok(new
            {
                Ticket = ToReadDto(ticket),
                Messages = messages.Select(m => new TicketMessageReadDto
                {
                    Id = m.Id,
                    TicketId = m.TicketId,
                    SenderUserId = m.SenderUserId,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList()
            });
        }

        /// <summary>
        /// Assigns a ticket to the current user and marks it as InProgress.
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        [HttpPost("{id}/assign")]
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.Rengøring)]
        public async Task<IActionResult> Assign(int id)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();
            if (!HasAccess(ticket)) return Forbid();

            ticket.AssignedToUserId = GetUserId();
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _hub.Clients.Group($"ticket-{ticket.Id}").SendAsync("TicketUpdated", ToReadDto(ticket));
            // Тост
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "Ticket assigned", Message = $"#{ticket.Id} taken into work", Level = "info", Ts = DateTime.UtcNow });
            return Ok(ToReadDto(ticket));
        }

        /// <summary>
        /// Updates the status of a ticket.
        /// </summary>
        /// <param name="id">Ticket identifier.</param>
        /// <param name="dto">New status payload.</param>
        [HttpPost("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] TicketStatusUpdateDto dto)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound();
            if (!HasAccess(ticket)) return Forbid();

            ticket.Status = dto.Status;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _hub.Clients.Group($"ticket-{ticket.Id}").SendAsync("TicketUpdated", ToReadDto(ticket));
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "Ticket status", Message = $"#{ticket.Id}: {dto.Status}", Level = "success", Ts = DateTime.UtcNow });
            await _notifier.SendAsync($"✅ Ticket #{ticket.Id} status: <b>{dto.Status}</b>");
            return Ok(ToReadDto(ticket));
        }

        /// <summary>
        /// Adds a message to a ticket via REST.
        /// </summary>
        /// <param name="dto">Message payload including ticket id.</param>
        /// <returns>The created ticket message.</returns>
        [HttpPost("messages")]
        public async Task<ActionResult<TicketMessageReadDto>> AddMessage([FromBody] TicketMessageCreateDto dto)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == dto.TicketId);
            if (ticket == null) return NotFound();
            if (!HasAccess(ticket)) return Forbid();

            var msg = new TicketMessage
            {
                TicketId = ticket.Id,
                SenderUserId = GetUserId(),
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.TicketMessages.Add(msg);
            await _db.SaveChangesAsync();
            var read = new TicketMessageReadDto
            {
                Id = msg.Id,
                TicketId = msg.TicketId,
                SenderUserId = msg.SenderUserId,
                Content = msg.Content,
                CreatedAt = msg.CreatedAt
            };
            await _hub.Clients.Group($"ticket-{ticket.Id}").SendAsync("NewMessage", read);
            return Ok(read);
        }

        private int GetUserId()
        {
            var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(id) || !int.TryParse(id, out var userId))
                throw new Exception("Invalid user.");
            return userId;
        }

        private bool HasAccess(Ticket t)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var uid))
            {
                if (t.CreatedByUserId == uid) return true;
                if (t.AssignedToUserId == uid) return true;
            }
            if (string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.IsNullOrEmpty(role) && string.Equals(role, t.TargetRoleName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static TicketReadDto ToReadDto(Ticket t) => new TicketReadDto
        {
            Id = t.Id,
            Title = t.Title,
            Category = t.Category,
            Status = t.Status,
            TargetRoleName = t.TargetRoleName,
            CreatedByUserId = t.CreatedByUserId,
            AssignedToUserId = t.AssignedToUserId,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}
