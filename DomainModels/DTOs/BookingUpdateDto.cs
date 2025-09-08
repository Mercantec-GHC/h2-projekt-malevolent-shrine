namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class BookingUpdateDto
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [Required]
    public DateTime CheckInDate { get; set; }  

    [Required]
    public DateTime CheckOutDate { get; set; } 

    [Required]
    [Range(0.01, 999999)]
    public decimal TotalPrice { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Pending";
}