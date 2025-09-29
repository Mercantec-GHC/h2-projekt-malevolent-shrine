// filepath: /Users/deuswork/RiderProjects/h2-projekt-malevolent-shrine/API/DTOs/CleaningTaskDtos.cs
using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs
{
    /// <summary>
    /// Payload to create a cleaning task.
    /// </summary>
    public class CleaningTaskCreateDto
    {
        /// <summary>
        /// Short title of the task.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Optional detailed description.
        /// </summary>
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        /// <summary>
        /// Optional room identifier the task is related to.
        /// </summary>
        public int? RoomId { get; set; }
        
        /// <summary>
        /// User id of the cleaner assigned to the task.
        /// </summary>
        [Required]
        public int AssignedToUserId { get; set; }
        
        /// <summary>
        /// Optional due date for the task.
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    /// <summary>
    /// Read model representing a cleaning task.
    /// </summary>
    public class CleaningTaskReadDto
    {
        /// <summary>
        /// Task identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Optional description.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Associated room id if any.
        /// </summary>
        public int? RoomId { get; set; }
        /// <summary>
        /// Assigned cleaner user id.
        /// </summary>
        public int AssignedToUserId { get; set; }
        /// <summary>
        /// Creator user id.
        /// </summary>
        public int CreatedByUserId { get; set; }
        /// <summary>
        /// Optional due date.
        /// </summary>
        public DateTime? DueDate { get; set; }
        /// <summary>
        /// Current status.
        /// </summary>
        public CleaningTaskStatus Status { get; set; }
        /// <summary>
        /// UTC timestamp when created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Payload to update cleaning task status.
    /// </summary>
    public class CleaningTaskStatusUpdateDto
    {
        /// <summary>
        /// New status value.
        /// </summary>
        [Required]
        public CleaningTaskStatus Status { get; set; }
    }
}
