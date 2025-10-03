namespace API.DTOs
{
    /// <summary>
    /// Request payload for logging in via Active Directory (LDAP).
    /// </summary>
    public class AdLoginRequestDto
    {
        /// <summary>
        /// AD username (sAMAccountName or UPN depending on setup).
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// AD user password.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response payload returned after a successful AD login.
    /// </summary>
    public class AdLoginResponseDto
    {
        /// <summary>
        /// Informational message about the login process.
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Issued JWT Access Token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>
        /// Access token expiration (UTC).
        /// </summary>
        public DateTime AccessTokenExpiry { get; set; }
        /// <summary>
        /// User e-mail from AD if available.
        /// </summary>
        public string? Email { get; set; }
        /// <summary>
        /// User display name from AD if available.
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Username used in the application context.
        /// </summary>
        public string? Username { get; set; }
        /// <summary>
        /// List of AD group names the user is a member of.
        /// </summary>
        public List<string> AdGroups { get; set; } = new();
        /// <summary>
        /// Application role resolved from AD groups.
        /// </summary>
        public string? AppRole { get; set; }
        /// <summary>
        /// Issued refresh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// Refresh token expiration (UTC).
        /// </summary>
        public DateTime RefreshTokenExpiry { get; set; }
    }
}