namespace DomainModels.Models;

public class Room : Common
{
    public required string Number { get; set; }
    public int Capacity { get; set; }

    // Навигационное свойство
    public Hotel? Hotel { get; set; }
    
    public bool IsAvailable { get; set; }
    public decimal PricePerNight { get; set; }
    
    public int Floor { get; set; }
    public int? HotelId { get; set; } 

    
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
}