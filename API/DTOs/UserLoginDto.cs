using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload required to authenticate a user via local login.
    /// </summary>
    public class UserLoginDto
    {
        /// <summary>
        /// E-mail address or username.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Plain-text password.
        /// </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}