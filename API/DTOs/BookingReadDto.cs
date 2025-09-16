namespace API.DTOs;

public class BookingReadDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; } 
    public DateTime CheckOutDate { get; set; }   
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}