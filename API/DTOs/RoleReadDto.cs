using System;

namespace API.DTOs
{
    /// <summary>
    /// Represents role details exposed to clients.
    /// </summary>
    public class RoleReadDto
    {
        /// <summary>
        /// Role identifier.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Creation timestamp (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Last update timestamp (UTC).
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}