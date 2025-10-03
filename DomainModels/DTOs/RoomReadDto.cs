namespace DomainModels.DTOs;
public class RoomReadDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerNight { get; set; }
    public int Floor { get; set; }
    public bool IsAvailable { get; set; }
    public int? HotelId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}