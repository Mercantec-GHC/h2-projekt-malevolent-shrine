using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using Microsoft.AspNetCore.Identity;


namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthController(AppDBContext context, JwtService jwtService, PasswordHasher<User> passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthDto request)
        {
            // Проверяем, существует ли уже пользователь с таким же именем
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Пользователь с таким именем уже существует.");
            }
            

            // Хешируем пароль с помощью BCrypt
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Создаем нового пользователя
            var newUser = new User
            {
                Username = request.Username,
                HashedPassword = hashedPassword,
                Email = request.Email,
                FirstName = request.FirstName, // из DTO
                LastName = request.LastName,   // из DTO
                RoleId = 4,
            };

            // Добавляем пользователя в базу данных
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("Регистрация успешна!");
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto request)
        {
            // ищу пользователя в базе данных по имени
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            // проверка что пользователь существует
            if (user == null)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            // совпадает ли пароль
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return BadRequest("Неверный логин или пароль.");
            }
            
            // Генерируем JWT токен если все ок
            var token = _jwtService.GenerateToken(user);

            // Возвращаем токен клиенту
            return Ok(new { Message = "Вход выполнен успешно!", Token = token });
        }
    }
        
}