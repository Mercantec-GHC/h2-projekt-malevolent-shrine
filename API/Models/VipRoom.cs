namespace API.Models;

public class VipRoom:Room
{
    public required string VipServiceDescription { get; set; }
    
    public List<string> ExtraAmenities { get; set; } 
}