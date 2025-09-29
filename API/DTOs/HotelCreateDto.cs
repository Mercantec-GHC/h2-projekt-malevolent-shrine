using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    /// <summary>
    /// Represents the payload to create a new hotel.
    /// </summary>
    public class HotelCreateDto
    {
        /// <summary>
        /// Hotel name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        /// <summary>
        /// Optional address of the hotel.
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }
    }
}