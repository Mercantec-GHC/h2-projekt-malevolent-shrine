namespace API.DTOs;

/// <summary>
/// Represents booking details returned to clients.
/// </summary>
public class BookingReadDto
{
    /// <summary>
    /// Unique identifier of the booking.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Identifier of the user who made the booking.
    /// </summary>
    public int UserId { get; set; }
    /// <summary>
    /// Identifier of the booked room.
    /// </summary>
    public int RoomId { get; set; }
    /// <summary>
    /// Check-in date.
    /// </summary>
    public DateTime CheckInDate { get; set; } 
    /// <summary>
    /// Check-out date.
    /// </summary>
    public DateTime CheckOutDate { get; set; }   
    /// <summary>
    /// Total price of the stay.
    /// </summary>
    public decimal TotalPrice { get; set; }
    /// <summary>
    /// Current status of the booking.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the booking was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public string? RoomNumber { get; set; }

    public string HotelName { get; set; }

}