namespace API.DTOs
{
    // DTO для запроса логина через AD
    public class AdLoginRequestDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO для ответа после логина через AD
    public class AdLoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Username { get; set; }
        public List<string> AdGroups { get; set; } = new();
        public string? AppRole { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiry { get; set; }
    }
}