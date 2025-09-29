namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to update an existing room.
/// </summary>
public class RoomUpdateDto
{
    /// <summary>
    /// Room identifier.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Room number (unique within a hotel).
    /// </summary>
    [Required]
    [StringLength(10)]
    public required string Number { get; set; }
    
    /// <summary>
    /// Maximum number of guests.
    /// </summary>
    [Range(1, 10)]
    public int Capacity { get; set; }
    
    /// <summary>
    /// Price per night.
    /// </summary>
    [Range(0.01, 50000)]
    public decimal PricePerNight { get; set; }
    
    /// <summary>
    /// Floor number.
    /// </summary>
    [Range(1, 100)]
    public int Floor { get; set; }
    
    /// <summary>
    /// Whether the room is available for booking.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Owning hotel identifier, if available.
    /// </summary>
    public int? HotelId { get; set; }
}