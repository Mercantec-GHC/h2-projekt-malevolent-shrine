namespace DomainModels.DTOs
{
    /// <summary>
    /// DTO for reading VIP room data
    /// </summary>
    public class VipRoomReadDto
    {
        /// <summary>
        /// VIP room ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Room number
        /// </summary>
        public string Number { get; set; } = string.Empty;
        
        /// <summary>
        /// Room capacity
        /// </summary>
        public int Capacity { get; set; }
        
        /// <summary>
        /// Price per night
        /// </summary>
        public decimal PricePerNight { get; set; }
        
        /// <summary>
        /// Floor number
        /// </summary>
        public int Floor { get; set; }
        
        /// <summary>
        /// Availability status
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Hotel ID
        /// </summary>
        public int? HotelId { get; set; }
        
        /// <summary>
        /// VIP service description
        /// </summary>
        public string VipServiceDescription { get; set; } = string.Empty;
        
        /// <summary>
        /// Extra amenities list
        /// </summary>
        public List<string> ExtraAmenities { get; set; } = new();
        
        /// <summary>
        /// Room description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Room image URL
        /// </summary>
        public string? ImageUrl { get; set; }
        
        /// <summary>
        /// Creation date
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Last update date
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
