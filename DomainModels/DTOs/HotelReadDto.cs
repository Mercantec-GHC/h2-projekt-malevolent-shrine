namespace DomainModels.DTOs
{
    public class HotelReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        public List<RoomReadDto> Rooms { get; set; } = new();
    }
}