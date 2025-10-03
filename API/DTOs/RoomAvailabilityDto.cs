namespace API.DTOs;

public class AvailabilityDayDto
{
    public DateTime Date { get; set; }
    public bool IsOccupied { get; set; }
    public int? BookingId { get; set; }
}

public class RoomAvailabilityDto
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int? HotelId { get; set; }
    public List<AvailabilityDayDto> Days { get; set; } = new();
}

