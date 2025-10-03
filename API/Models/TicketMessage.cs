using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    // Простое сообщение в тикете (элемент чата)
    public class TicketMessage : Common
    {
        [Required]
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [Required]
        public int SenderUserId { get; set; }
        public User SenderUser { get; set; } = null!;

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
    }
}
