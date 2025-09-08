using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class HotelCreateDto
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
    }
}