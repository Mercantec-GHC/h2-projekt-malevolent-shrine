using System.Security.Claims;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;

        public AuthController(AppDBContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Приводим email к нижнему регистру
            if (!string.IsNullOrWhiteSpace(request.Email))
                request.Email = request.Email.ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest("Пользователь с таким именем уже существует.");

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Пользователь с таким email уже существует.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                HashedPassword = hashedPassword,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                RoleId = 4,
            };
           
            _context.Users.Add(newUser);     // Добавляем пользователя в базу данных TODO: только если пользователь еще не существует
            await _context.SaveChangesAsync();

            return Ok("Регистрация успешна!");
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Приводим email к нижнему регистру
            if (!string.IsNullOrWhiteSpace(request.Email))
                request.Email = request.Email.ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return BadRequest("Неверный логин или пароль.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                return BadRequest("Неверный логин или пароль.");

            var accessToken = _jwtService.GenerateToken(user, user.Role.Name);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = _jwtService.GetRefreshTokenExpiry(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
// Ограничиваем количество активных токенов на пользователя (например, 5)
            var activeTokensCount = user.RefreshTokens.Count(t => t.IsActive);
            if (activeTokensCount >= 5)
            {
                var oldestActive = user.RefreshTokens
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.CreatedAt)
                    .First();
                
                oldestActive.IsRevoked = true;
                oldestActive.RevokedAt = DateTime.UtcNow;
                oldestActive.RevokedByIp = "exceeded_limit";
            }
            

            user.RefreshTokens.Add(refreshTokenEntity);

            // Удаляем старые неактивные токены
            var inactiveTokens = user.RefreshTokens
                .Where(t => !t.IsActive)
                .ToList();

            foreach (var token in inactiveTokens)
            {
                _context.RefreshTokens.Remove(token);
                user.RefreshTokens.Remove(token); 
                
            }

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
                RefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry(),
                Message = "Вход выполнен успешно!"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refreshToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .ThenInclude(u => u.Role)
                .Include(rt => rt.User)
                .ThenInclude(u => u.RefreshTokens)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);


            
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return BadRequest("Недействительный refresh token.");
            }

            // Отзываем старый токен
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Генерируем новые токены
            var newAccessToken = _jwtService.GenerateToken(refreshToken.User, refreshToken.User.Role.Name);
            var newRefreshToken = _jwtService.GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                
                Token = newRefreshToken,
                ExpiryDate = _jwtService.GetRefreshTokenExpiry(),
                UserId = refreshToken.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
                
            };

            // Помечаем, что старый был заменён
            refreshToken.ReplacedByToken = newRefreshToken;

            // Добавляем новый токен к пользователю
            refreshToken.User.RefreshTokens.Add(newRefreshTokenEntity);

            // Удаляем старые неактивные токены (кроме только-что добавленного)
            var user = refreshToken.User;
            var inactiveTokens = user.RefreshTokens
                .Where(t => !t.IsActive && t.Token != newRefreshToken)
                .ToList();

            foreach (var token in inactiveTokens)
            {
                
                _context.RefreshTokens.Remove(token);
                user.RefreshTokens.Remove(token); 
                
            }

            await _context.SaveChangesAsync();

            return Ok(new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                AccessTokenExpiry = _jwtService.GetAccessTokenExpiry(),
                RefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry(),
                Message = "Токены успешно обновлены."
            });
        }
        
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (refreshToken == null || !refreshToken.IsActive)
                return BadRequest("Недействительный refresh token.");

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _context.SaveChangesAsync();

            return Ok("Токен успешно отозван.");
        }
[HttpPost("revoke-all-tokens")]
        public async Task<IActionResult> RevokeAllTokens()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Недействительный пользователь.");
            }
        
            var user = await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);
        
            if (user == null)
                return BadRequest("Пользователь не найден.");
        
            foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
        
            await _context.SaveChangesAsync();
            return Ok("Все токены успешно отозваны.");
        }
        
    }
}