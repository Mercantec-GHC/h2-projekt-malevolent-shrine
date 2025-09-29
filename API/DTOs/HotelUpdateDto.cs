namespace API.DTOs
{
    /// <summary>
    /// Represents the payload for updating an existing hotel.
    /// </summary>
    public class HotelUpdateDto
    {
        /// <summary>
        /// Hotel identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Updated hotel name.
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Updated address; optional.
        /// </summary>
        public string? Address { get; set; }
    }
}