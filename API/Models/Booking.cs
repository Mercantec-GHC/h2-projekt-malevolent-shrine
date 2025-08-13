namespace API.Models;

public class Booking: Common
{
    public int UserId { get; set; }
    public UserInfo User { get; set; } = default!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    
    public int? BookingId { get; set; }
}