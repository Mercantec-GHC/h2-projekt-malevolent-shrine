using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;

        public AuthController(AppDBContext context)
        {
            _context = context;
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
                HashedPassword = System.Text.Encoding.UTF8.GetBytes(hashedPassword),
                Email = request.Email, // без этого поля не получится зарегистрироваться
                RoleId = 4, // Присваиваем роль "Kunde" (клиент) по умолчанию
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
            if (!BCrypt.Net.BCrypt.Verify(request.Password, System.Text.Encoding.UTF8.GetString(user.HashedPassword)))
            {
                return BadRequest("Неверный логин или пароль.");
            }

            // Если все хорошо, возвращаем сообщение об успехе
            return Ok("Вход выполнен успешно!");
        }
    }
        
}