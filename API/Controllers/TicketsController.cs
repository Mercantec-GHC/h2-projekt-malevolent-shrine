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
    // Контроллер тикетов: супер-простой и понятный
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDBContext _db;
        private readonly TicketRoutingService _routing;
        private readonly IHubContext<TicketHub> _hub;
        private readonly TelegramNotifier _notifier;

        public TicketsController(AppDBContext db, TicketRoutingService routing, IHubContext<TicketHub> hub, TelegramNotifier notifier)
        {
            _db = db;
            _routing = routing;
            _hub = hub;
            _notifier = notifier;
        }

        // 1) Создать тикет: создаём запись и первое сообщение, рассылаем событие в группу роли
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
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "Новый тикет", Message = $"#{ticket.Id} {ticket.Title} → {targetRole}", Level = "info", Ts = DateTime.UtcNow });
            // Telegram уведомление (если настроено)
            await _notifier.SendAsync($"🆕 Тикет #{ticket.Id} — <b>{ticket.Title}</b> ({ticket.Category}) для роли <b>{targetRole}</b>");

            return Ok(read);
        }

        // 2) Мои тикеты (клиент видит только свои)
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

        // 3) Тикеты для моей роли (сотрудник видит по своей роли, админ — всё)
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

        // 4) Детали тикета + последние N сообщений (просто 50)
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

        // 5) Сотрудник берёт тикет в работу
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
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "Тикет взят", Message = $"#{ticket.Id} взят в работу", Level = "info", Ts = DateTime.UtcNow });
            return Ok(ToReadDto(ticket));
        }

        // 6) Смена статуса
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
            await _hub.Clients.Group("admins").SendAsync("toast", new { Title = "Статус тикета", Message = $"#{ticket.Id}: {dto.Status}", Level = "success", Ts = DateTime.UtcNow });
            await _notifier.SendAsync($"✅ Тикет #{ticket.Id} статус: <b>{dto.Status}</b>");
            return Ok(ToReadDto(ticket));
        }

        // 7) Добавить сообщение через REST (альтернатива хабу)
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
                throw new Exception("Некорректный пользователь.");
            return userId;
        }

        private bool HasAccess(Ticket t)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out var uid))
            {
                if (t.CreatedByUserId == uid) return true; // владелец тикета
                if (t.AssignedToUserId == uid) return true; // исполнитель
            }
            if (string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)) return true; // админ
            if (!string.IsNullOrEmpty(role) && string.Equals(role, t.TargetRoleName, StringComparison.OrdinalIgnoreCase)) return true; // сотрудник нужной роли
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
