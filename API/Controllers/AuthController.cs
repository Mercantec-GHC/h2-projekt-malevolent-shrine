using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Services;
using API.DTOs;
using Microsoft.AspNetCore.Identity;


namespace API.Controllers
{
    /// <summary>
    /// Kontroller til håndtering af brugerautentificering, herunder registrering og login.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;

        /// <summary>
        /// Initialiserer en ny instans af AuthController med databasekontekst, JWT-service og password-hasher.
        /// </summary>
        /// <param name="context">Databasekontekst til brugere og roller.</param>
        /// <param name="jwtService">Service til generering af JWT-tokens.</param>
        /// <param name="passwordHasher">Hasher til beskyttelse af brugernes adgangskoder.</param>
        public AuthController(AppDBContext context, JwtService jwtService, PasswordHasher<User> passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Registrerer en ny bruger i systemet.
        /// </summary>
        /// <param name="request">DTO med oplysninger om brugernavn, adgangskode, email og navn.</param>
        /// <returns>En succesbesked, hvis brugeren er registreret korrekt, ellers fejlmeddelelse.</returns>
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

        /// <summary>
        /// Logger en eksisterende bruger ind og returnerer et JWT-token.
        /// </summary>
        /// <param name="request">DTO med email og adgangskode.</param>
        /// <returns>JWT-token, hvis login er vellykket, ellers fejlmeddelelse.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto request)
        {
            // Ищем пользователя с ролью
            var user = await _context.Users
                .Include(u => u.Role) // Загружаем роль для JWT
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Неверный логин или пароль.");
            }

            // Проверка пароля
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
            {
                return BadRequest("Неверный логин или пароль.");
            }

            var token = _jwtService.GenerateToken(user, user.Role?.Name);
            return Ok(new { Message = "Вход выполнен успешно!", Token = token });
        }
    }

}