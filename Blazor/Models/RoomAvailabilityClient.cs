namespace Blazor.Models;

public class AvailabilityDayVM
{
    public DateTime Date { get; set; }
    public bool IsOccupied { get; set; }
    public int? BookingId { get; set; }
}

public class RoomAvailabilityVM
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int? HotelId { get; set; }
    public List<AvailabilityDayVM> Days { get; set; } = new();
}

