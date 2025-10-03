namespace Blazor.Models
{
    // Копии простых моделей для клиента (чтобы не тянуть серверные DTO)
    public enum TicketCategory { Cleaning = 0, Technical = 1, General = 2 }
    public enum TicketStatus { Open = 0, InProgress = 1, Resolved = 2, Closed = 3 }

    public class TicketCreate
    {
        public string Title { get; set; } = string.Empty;
        public TicketCategory Category { get; set; } = TicketCategory.General;
        public string Description { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public int? RoomId { get; set; }
    }

    public class TicketRead
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

    public class TicketStatusUpdate { public TicketStatus Status { get; set; } }

    public class TicketMessageCreate { public int TicketId { get; set; } public string Content { get; set; } = string.Empty; }

    public class TicketMessageRead
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public int SenderUserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TicketWithMessages
    {
        public TicketRead Ticket { get; set; } = new TicketRead();
        public List<TicketMessageRead> Messages { get; set; } = new();
    }
}
