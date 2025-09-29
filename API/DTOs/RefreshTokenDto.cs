using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload containing a refresh token.
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// The refresh token string.
        /// </summary>
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}