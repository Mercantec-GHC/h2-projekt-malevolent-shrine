namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class RoomUpdateDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(10)]
    public required string Number { get; set; }
    
    [Range(1, 10)]
    public int Capacity { get; set; }
    
    [Range(0.01, 50000)]
    public decimal PricePerNight { get; set; }
    
    [Range(1, 100)]
    public int Floor { get; set; }
    
    public bool IsAvailable { get; set; }
    public int? HotelId { get; set; }
}