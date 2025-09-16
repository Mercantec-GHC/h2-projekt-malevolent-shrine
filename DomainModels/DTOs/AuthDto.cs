using System.ComponentModel.DataAnnotations;

namespace DomainModels.Models

{
    public class AuthDto
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        
        [Required]
        public  string FirstName { get; set; } 
        
        [Required]
        public  string LastName { get; set; }
    }
}