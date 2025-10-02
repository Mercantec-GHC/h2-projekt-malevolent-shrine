namespace API.DTOs;

/// <summary>
/// Represents a room returned to clients.
/// </summary>
public class RoomReadDto
{
    /// <summary>
    /// Room identifier.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Room number.
    /// </summary>
    public string Number { get; set; } = string.Empty;
    /// <summary>
    /// Maximum number of guests.
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// Price per night.
    /// </summary>
    public decimal PricePerNight { get; set; }
    /// <summary>
    /// Floor number.
    /// </summary>
    public int Floor { get; set; }
    /// <summary>
    /// Whether the room is available for booking.
    /// </summary>
    public bool IsAvailable { get; set; }
    /// <summary>
    /// Owning hotel identifier, if available.
    /// </summary>
    public int? HotelId { get; set; }

    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}