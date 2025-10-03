using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload required to register a new user via the local authentication flow.
    /// </summary>
    public class AuthDto
    {
        /// <summary>
        /// Desired username for the new account.
        /// </summary>
        /// <remarks>
        /// Must be unique across users.
        /// </remarks>
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password to be hashed and stored for the account.
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Email address of the user.
        /// </summary>
        /// <remarks>
        /// Used for login and communication. Must be unique.
        /// </remarks>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// User's given name.
        /// </summary>
        [Required]
        public  string FirstName { get; set; } = string.Empty; 
        
        /// <summary>
        /// User's family name (surname).
        /// </summary>
        [Required]
        public  string LastName { get; set; } = string.Empty;
    }
}