using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.DTOs;
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
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            // Ищите по Email, а не по Username
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    
            if (user == null)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            {
                return BadRequest("Неверный логин или пароль.");
            }

            var token = _jwtService.GenerateToken(user);
            return Ok(new { Message = "Вход выполнен успешно!", Token = token });
        }
    }
        
}