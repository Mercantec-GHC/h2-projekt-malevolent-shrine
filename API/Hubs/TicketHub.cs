using System.Security.Claims;
using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Hubs
{
    // Hub для чата в тикетах
    [Authorize]
    public class TicketHub : Hub
    {
        private readonly AppDBContext _db;
        public TicketHub(AppDBContext db)
        {
            _db = db;
        }

        // При подключении подписываем сотрудника на группу своей роли, чтобы ловить новые тикеты
        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role-{role}");
            }
            await base.OnConnectedAsync();
        }

        // Клиент вызывает, чтобы получать сообщения конкретного тикета
        public async Task JoinTicket(int ticketId)
        {
            // Простая проверка доступа
            if (!await HasAccessToTicket(ticketId))
                throw new HubException("Нет доступа к этому тикету.");

            await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        public async Task LeaveTicket(int ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
        }

        // Отправка сообщения в тикет из хаба (чат в реальном времени)
        public async Task SendMessage(int ticketId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new HubException("Пустое сообщение.");

            var userId = GetUserId();
            if (!await HasAccessToTicket(ticketId))
                throw new HubException("Нет доступа к этому тикету.");

            var message = new TicketMessage
            {
                TicketId = ticketId,
                SenderUserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.TicketMessages.Add(message);
            await _db.SaveChangesAsync();

            var dto = new TicketMessageReadDto
            {
                Id = message.Id,
                TicketId = message.TicketId,
                SenderUserId = message.SenderUserId,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            };

            await Clients.Group($"ticket-{ticketId}").SendAsync("NewMessage", dto);
        }

        private int GetUserId()
        {
            var idStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var userId))
                throw new HubException("Некорректный пользователь.");
            return userId;
        }

        private async Task<bool> HasAccessToTicket(int ticketId)
        {
            var ticket = await _db.Tickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId);
            if (ticket == null) return false;
            var userId = GetUserId();
            if (ticket.CreatedByUserId == userId) return true;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (string.Equals(role, RoleNames.Admin, StringComparison.OrdinalIgnoreCase)) return true;
            if (!string.IsNullOrEmpty(role) && string.Equals(ticket.TargetRoleName, role, StringComparison.OrdinalIgnoreCase)) return true;
            if (ticket.AssignedToUserId == userId) return true;
            return false;
        }
    }
}
