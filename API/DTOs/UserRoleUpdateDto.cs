using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class UserRoleUpdateDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, 5, ErrorMessage = "Role ID must be between 1 and 5")]
        public int RoleId { get; set; }
    }
}
