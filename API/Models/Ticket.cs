using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    // простая модель тикета (обращения)
    public enum TicketCategory
    {
        Cleaning = 0,
        Technical = 1,
        General = 2
    }

    public enum TicketStatus
    {
        Open = 0,
        InProgress = 1,
        Resolved = 2,
        Closed = 3
    }

    public class Ticket : Common
    {
        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty; // Короткое название проблемы

        public TicketCategory Category { get; set; } = TicketCategory.General; // Категория
        public TicketStatus Status { get; set; } = TicketStatus.Open;          // Статус

        public int CreatedByUserId { get; set; } // Кто создал тикет (пользователь)
        public User CreatedByUser { get; set; } = null!;

        public int? BookingId { get; set; } // Не обязательно
        public int? RoomId { get; set; }    // Не обязательно

        // Кому адресован тикет (по умолчанию по категории)
        public string TargetRoleName { get; set; } = "Manager"; // Например: "Rengøring" или "Manager"

        // Кто взял тикет в работу (сотрудник)
        public int? AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        // Сообщения (как чат)
        public List<TicketMessage> Messages { get; set; } = new();
    }
}
