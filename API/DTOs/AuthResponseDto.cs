namespace API.DTOs
{
    /// <summary>
    /// Represents the standard response after authentication or token refresh.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// JWT access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>
        /// Refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// Expiration timestamp for the access token (UTC).
        /// </summary>
        public DateTime AccessTokenExpiry { get; set; }
        /// <summary>
        /// Expiration timestamp for the refresh token (UTC).
        /// </summary>
        public DateTime RefreshTokenExpiry { get; set; }
        /// <summary>
        /// Human-readable message about the authentication result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}