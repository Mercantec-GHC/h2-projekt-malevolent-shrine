namespace DomainModels.Models;

public class Hotel : Common
{
    public required string Name { get; set; }
    public string Address { get; set; } = "";

    public List<Room> Rooms { get; set; } = new(); // 1:n
}