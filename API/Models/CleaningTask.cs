
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public enum CleaningTaskStatus
    {
        New = 0,
        InProgress = 1,
        Done = 2
    }

    public class CleaningTask : Common
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        
        [Required]
        public int AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }
        
        [Required]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Required]
        public CleaningTaskStatus Status { get; set; } = CleaningTaskStatus.New;
    }
}
