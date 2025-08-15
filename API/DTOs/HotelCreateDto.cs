using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class HotelCreateDto
    {
        public required string Name { get; set; }
        
        public string? Address { get; set; }
    }
}