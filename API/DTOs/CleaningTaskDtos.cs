// filepath: /Users/deuswork/RiderProjects/h2-projekt-malevolent-shrine/API/DTOs/CleaningTaskDtos.cs
using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs
{
    public class CleaningTaskCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        public int? RoomId { get; set; }
        
        [Required]
        public int AssignedToUserId { get; set; }
        
        public DateTime? DueDate { get; set; }
    }

    public class CleaningTaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? RoomId { get; set; }
        public int AssignedToUserId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DueDate { get; set; }
        public CleaningTaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CleaningTaskStatusUpdateDto
    {
        [Required]
        public CleaningTaskStatus Status { get; set; }
    }
}
