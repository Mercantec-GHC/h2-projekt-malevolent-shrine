using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload to change a user's role.
    /// </summary>
    public class UserRoleUpdateDto
    {
        /// <summary>
        /// Target user identifier.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        /// <summary>
        /// New role identifier to assign.
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, 5, ErrorMessage = "Role ID must be between 1 and 5")]
        public int RoleId { get; set; }
    }
}
