namespace API.DTOs
{
    /// <summary>
    /// Read model exposing basic user information.
    /// </summary>
    public class UserReadDto
    {
        /// <summary>
        /// User identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Given name.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;
        /// <summary>
        /// Family name (surname).
        /// </summary>
        public string LastName { get; set; } = string.Empty;
        /// <summary>
        /// Primary e-mail address.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>
        /// Contact phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
        /// <summary>
        /// Optional mailing address.
        /// </summary>
        public string? Address { get; set; }
    }
}