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

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
            _secretKey = _configuration["Jwt:SecretKey"] ??
                         Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                         "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong123456789";
            _issuer = _configuration["Jwt:Issuer"] ??
                      Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                      "H2-2025-API";
            _audience = _configuration["Jwt:Audience"] ??
                        Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                        "H2-2025-Client";
            _expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ??
                                       Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "60");
        }
        
        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name) //у User есть свойство Role
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.Now.AddMinutes(_expiryMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}