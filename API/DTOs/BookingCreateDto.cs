namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to create a new booking.
/// </summary>
public class BookingCreateDto
{
    /// <summary>
    /// Identifier of the user making the booking.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Identifier of the room to be booked.
    /// </summary>
    [Required]
    public int RoomId { get; set; }

    /// <summary>
    /// Check-in date.
    /// </summary>
    [Required]
    public DateTime CheckInDate { get; set; }  

    /// <summary>
    /// Check-out date.
    /// </summary>
    [Required]
    public DateTime CheckOutDate { get; set; } 

    /// <summary>
    /// Total price for the stay.
    /// </summary>
    [Required]
    [Range(0.01, 999999)]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Initial status of the booking.
    /// </summary>
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}