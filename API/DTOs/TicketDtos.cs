using API.Models;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Payload to create a support ticket.
    /// </summary>
    public class TicketCreateDto
    {
        /// <summary>
        /// Short title describing the issue.
        /// </summary>
        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Ticket category (e.g., Cleaning, Technical, General).
        /// </summary>
        [Required]
        public TicketCategory Category { get; set; } = TicketCategory.General;

        /// <summary>
        /// Detailed problem description that becomes the first message.
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional related booking identifier.
        /// </summary>
        public int? BookingId { get; set; }

        /// <summary>
        /// Optional related room identifier.
        /// </summary>
        public int? RoomId { get; set; }
    }

    /// <summary>
    /// Read model with basic ticket information.
    /// </summary>
    public class TicketReadDto
    {
        /// <summary>
        /// Ticket identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Ticket title.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Ticket category.
        /// </summary>
        public TicketCategory Category { get; set; }
        /// <summary>
        /// Current status of the ticket.
        /// </summary>
        public TicketStatus Status { get; set; }
        /// <summary>
        /// Name of the role responsible for handling the ticket.
        /// </summary>
        public string TargetRoleName { get; set; } = string.Empty;
        /// <summary>
        /// Identifier of the user who created the ticket.
        /// </summary>
        public int CreatedByUserId { get; set; }
        /// <summary>
        /// Identifier of the user assigned to the ticket, if any.
        /// </summary>
        public int? AssignedToUserId { get; set; }
        /// <summary>
        /// UTC timestamp when the ticket was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// UTC timestamp when the ticket was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Payload to update the status of a ticket.
    /// </summary>
    public class TicketStatusUpdateDto
    {
        /// <summary>
        /// New ticket status.
        /// </summary>
        [Required]
        public TicketStatus Status { get; set; }
    }

    /// <summary>
    /// Payload to assign a ticket to the current user.
    /// </summary>
    public class TicketAssignDto
    {
        // Intentionally empty; the current authenticated user is used as the assignee.
    }

    /// <summary>
    /// Payload to create a new message within a ticket conversation.
    /// </summary>
    public class TicketMessageCreateDto
    {
        /// <summary>
        /// Ticket identifier.
        /// </summary>
        [Required]
        public int TicketId { get; set; }
        /// <summary>
        /// Message content.
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Read model for a ticket message.
    /// </summary>
    public class TicketMessageReadDto
    {
        /// <summary>
        /// Message identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Parent ticket identifier.
        /// </summary>
        public int TicketId { get; set; }
        /// <summary>
        /// Sender user identifier.
        /// </summary>
        public int SenderUserId { get; set; }
        /// <summary>
        /// Message content.
        /// </summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>
        /// UTC timestamp when the message was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
