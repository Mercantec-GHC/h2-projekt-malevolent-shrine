using System.Collections.Generic;

namespace API.DTOs
{
    /// <summary>
    /// Represents a hotel with its basic details and available rooms.
    /// </summary>
    public class HotelReadDto
    {
        /// <summary>
        /// Hotel identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Hotel name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Hotel address.
        /// </summary>
        public string Address { get; set; } = string.Empty;
        
        /// <summary>
        /// Rooms available in the hotel.
        /// </summary>
        public List<RoomReadDto> Rooms { get; set; } = new();
    }
}