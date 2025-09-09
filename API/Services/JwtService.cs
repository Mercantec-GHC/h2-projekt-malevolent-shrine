using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Models;

namespace API.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expiryMinutes;
        private readonly int _refreshTokenExpiryDays;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ??
                         Environment.GetEnvironmentVariable("Jwt_SecretKey") ??
                         "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";
            // Проверка на null или слишком короткий ключ
            if (string.IsNullOrWhiteSpace(_secretKey) || _secretKey.Length < 32)
            {
                throw new ArgumentException("JWT SecretKey is missing or too short. Please set a secure key (at least 32 characters) in appsettings.json or environment variables.");
            }
            
            _issuer = _configuration["Jwt:Issuer"] ??
                      Environment.GetEnvironmentVariable("JWT_Issuer") ??
                      "H2-2025-API";
            _audience = _configuration["Jwt:Audience"] ??
                        Environment.GetEnvironmentVariable("Jwt_Audience") ??
                        "H2-2025-Client";
            _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ??
                                       Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60");
            _refreshTokenExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
            
        }
        
        public string GenerateToken(User user, string? roleName = null)
        {
            // Если роль не передана или пустая, это ошибка - не должно быть роли по умолчанию
            if (string.IsNullOrEmpty(roleName))
            {
                throw new InvalidOperationException($"Роль пользователя {user.Email} не загружена. Проверьте Include(u => u.Role) в запросе.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName) // Используем только переданную роль
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        
        public DateTime GetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);
        }

        public DateTime GetAccessTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_expiryMinutes);
        }
        
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}