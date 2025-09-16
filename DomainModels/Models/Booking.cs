namespace DomainModels.Models;

public class Booking : Common
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int RoomId { get; set; }
    public Room Room { get; set; } = default!;

    public DateTime CheckInDate { get; set; }  
    public DateTime CheckOutDate { get; set; } 
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
}