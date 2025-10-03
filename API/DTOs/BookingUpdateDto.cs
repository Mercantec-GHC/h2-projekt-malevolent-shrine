namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to update an existing booking.
/// </summary>
public class BookingUpdateDto
{
    /// <summary>
    /// Booking identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the user who made the booking.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Identifier of the booked room.
    /// </summary>
    [Required]
    public int RoomId { get; set; }

    /// <summary>
    /// New check-in date.
    /// </summary>
    [Required]
    public DateTime CheckInDate { get; set; }  

    /// <summary>
    /// New check-out date.
    /// </summary>
    [Required]
    public DateTime CheckOutDate { get; set; } 

    /// <summary>
    /// Updated total price for the stay.
    /// </summary>
    [Required]
    [Range(0.01, 999999)]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Updated status of the booking.
    /// </summary>
    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}