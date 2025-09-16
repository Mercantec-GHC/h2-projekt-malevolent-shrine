using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}