using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.DTOs;


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

            _context.Users.Add(newUser);
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
                // Если в модели есть CreatedAt/CreatedByIp — можно заполнить здесь
                // CreatedAt = DateTime.UtcNow,
                // CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            user.RefreshTokens.Add(refreshTokenEntity);

            // Удаляем старые неактивные токены
            var inactiveTokens = user.RefreshTokens
                .Where(t => !t.IsActive)
                .ToList();

            foreach (var token in inactiveTokens)
            {
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
                .ThenInclude(u => u.Role).Include(refreshToken => refreshToken.User)
                .ThenInclude(user => user.RefreshTokens)
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
                // CreatedAt = DateTime.UtcNow,
                // CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
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
    }
}
