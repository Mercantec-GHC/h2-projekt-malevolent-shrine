using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload to create a new room.
    /// </summary>
    public class RoomCreateDto
    {
        /// <summary>
        /// Room number (unique within a hotel).
        /// </summary>
        [Required]
        [StringLength(10)]
        public required string Number { get; set; }

        /// <summary>
        /// Maximum number of guests.
        /// </summary>
        [Range(1, 20)]
        public int Capacity { get; set; }

        /// <summary>
        /// Price per night.
        /// </summary>
        [Range(0.01, 10000)]
        public decimal PricePerNight { get; set; }

        /// <summary>
        /// Floor number.
        /// </summary>
        [Range(1, 100)]
        public int Floor { get; set; }

        /// <summary>
        /// Whether the room is available for booking.
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Owning hotel identifier.
        /// </summary>
        [Required]
        public int HotelId { get; set; }
    }
}