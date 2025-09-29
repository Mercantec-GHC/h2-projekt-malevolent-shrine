using API.Models;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    // Простой запрос на создание тикета
    public class TicketCreateDto
    {
        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty; // коротко: "Шумит кондиционер"
        [Required]
        public TicketCategory Category { get; set; } = TicketCategory.General; // Cleaning/Technical/General
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty; // первое сообщение тикета
        public int? BookingId { get; set; }
        public int? RoomId { get; set; }
    }

    // Ответ с базовой инфой о тикете
    public class TicketReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TicketCategory Category { get; set; }
        public TicketStatus Status { get; set; }
        public string TargetRoleName { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class TicketStatusUpdateDto
    {
        [Required]
        public TicketStatus Status { get; set; }
    }

    public class TicketAssignDto
    {
        // пусто — просто факт, что текущий сотрудник берёт тикет
    }

    public class TicketMessageCreateDto
    {
        [Required]
        public int TicketId { get; set; }
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    public class TicketMessageReadDto
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int SenderUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
