

namespace DomainModels.DTOs
{
    public class HotelUpdateDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Address { get; set; }
    }
}