using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// DTO for creating a new VIP room
    /// </summary>
    public class VipRoomCreateDto
    {
        /// <summary>
        /// Room number (unique identifier)
        /// </summary>
        [Required]
        public required string Number { get; set; }

        /// <summary>
        /// Maximum capacity of the room
        /// </summary>
        [Range(1, 20)]
        public int Capacity { get; set; }

        /// <summary>
        /// Price per night in currency
        /// </summary>
        [Range(0.01, 10000)]
        public decimal PricePerNight { get; set; }

        /// <summary>
        /// Floor number where the room is located
        /// </summary>
        public int Floor { get; set; }

        /// <summary>
        /// Whether the room is currently available for booking
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// ID of the hotel this room belongs to
        /// </summary>
        public int? HotelId { get; set; }

        /// <summary>
        /// Description of VIP services provided
        /// </summary>
        [Required]
        public required string VipServiceDescription { get; set; }

        /// <summary>
        /// List of extra amenities available in this VIP room
        /// </summary>
        public List<string> ExtraAmenities { get; set; } = new();

        /// <summary>
        /// Optional room description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Optional room image URL
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}
