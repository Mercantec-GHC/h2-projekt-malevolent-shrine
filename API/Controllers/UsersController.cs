using API.Data;
using API.DTOs;
using API.Models;
using API.Services;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace API.Controllers
{
    /// <summary>
    /// Controller til brugere.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<UsersController> _logger;
        
        
        /// <summary>
        /// Initialiserer UsersController med nødvendige services.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="jwtService"></param>
        /// <param name="logger"></param>
        /// /// <remarks>
        /// Dette er konstruktøren for UsersController, der bruger Dependency Injection til at få
        ///  AppDBContext, JwtService.
        ///  logger bruges til at logge informationer og fejl.
        ///  Dette sikrer, at controlleren har adgang til databasen, JWT-tjenesten
        ///  for at håndtere brugerautentificering og
        ///  autorisation.
        ///  Denne controller håndterer CRUD-operationer for brugere, herunder oprettelse,
        ///  læsning, opdatering og sletning af brugere.
        ///  Den er også ansvarlig for at hente den aktuelle bruger og ændre brugerroller
        ///  samt hente alle tilgængelige roller.
        /// </remarks>
        /// <returns> </returns>    
        public UsersController(AppDBContext context, JwtService jwtService, ILogger<UsersController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }
        
       /// <summary>    
       /// Henter alle brugere
       /// Kun for autoriserede brugere
       /// </summary>
       /// <returns>En liste af UserReadDto</returns>
      
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Добавляем InfiniteVoid
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserInfo) // Загружаем связанные данные!
                    .ToListAsync();

                var userReadDtos = users.Select(u => new UserReadDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.UserInfo?.PhoneNumber ?? "", // Через UserInfo!
                    Address = u.UserInfo?.Address ?? "" // Через UserInfo!
                }).ToList();

                return Ok(userReadDtos);
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при получении списка пользователей.");

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");
            }
            
        }

        /// <summary>
        ///  Hentér en specifik bruger
        ///  Kun for autoriserede brugere med roller Admin eller Manager
        /// </summary>
        /// <param name="id">Users unikke ID</param>
        /// <returns>En UserReadDTO hvis brugeren findes</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Добавляем InfiniteVoid
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserInfo) // Загружаем связанные данные!
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound();
                }

                var userReadDto = new UserReadDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.UserInfo?.PhoneNumber ?? "", // Через UserInfo!
                    Address = user.UserInfo?.Address ?? "" // Через UserInfo!
                };

                return Ok(userReadDto);
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при получении пользователя с ID {UserId}.", id);

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");
            }
        }

        /// <summary>
        ///  Opretter en ny bruger
        ///  Kun for autoriserede brugere med roller Admin eller Manager
        /// </summary>
        /// <param name="userDto"></param>
        /// <returns>Returnerer en UserReadDto med oprettede brugerens ID og detaljer</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]// Добавляем InfiniteVoid
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto userDto)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("Пользователь с таким email уже существует.");

            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return BadRequest("Пользователь с таким именем уже существует.");

            
            try
            {
                var user = new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    Username = userDto.Username,
                    HashedPassword = hashedPassword,
                    RoleId = 4, // Роль "Kunde" по умолчанию
                    UserInfo = new UserInfo
                    {
                        PhoneNumber = userDto.PhoneNumber,
                        Address = userDto.Address // Создаем UserInfo с данными из DTO
                    }
                };
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                var userReadDto = new UserReadDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.UserInfo.PhoneNumber, // Через UserInfo! 
                    Address = user.UserInfo.Address // Через UserInfo!
                };
                return CreatedAtAction(nameof(GetUser), new { id = userReadDto.Id }, userReadDto);
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при создании пользователя.");

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, $"Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже. {ex.Message}\n\n{ex}");
            }
        }
        
        /// <summary>
        /// Opdaterer en eksisterende bruger
        /// Kun for autoriserede brugere med roller Admin eller Manager
        /// </summary>
        /// <param name="id">Brugerens unikke ID</param>
        /// <param name="userDto">Brugerens opdaterede data</param>
        /// <returns>Returnerer NoContent hvis opdateringen lykkedes</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Добавляем InfiniteVoid
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            } 
            
            if (id != userDto.Id)
            {
                return BadRequest();
            }
            
            // Проверка уникальности email и username
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
                return BadRequest("Пользователь с таким email уже существует.");
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username && u.Id != id))
                return BadRequest("Пользователь с таким именем уже существует.");


            var user = await _context.Users
                .Include(u => u.UserInfo) // Загружаем связанные данные!
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            if (user.UserInfo == null)
            {
                user.UserInfo = new UserInfo(); // Создаем UserInfo, если его нет
            }

            user.UserInfo.PhoneNumber = userDto.PhoneNumber;
            user.UserInfo.Address = userDto.Address; // Обновляем данные UserInfo
            _context.Entry(user).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        
        /// <summary>
        /// Sletter en eksisterende bruger
        /// Kun for autoriserede brugere med roller Admin eller Manager
        /// </summary>
        /// <param name="id">Brugerens unikke ID</param>
        /// <returns>Returnerer NoContent hvis sletningen lykkedes</returns>
      
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Добавляем InfiniteVoid
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при удалении пользователя.");

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");
            }
            
        }

        /// <summary>
        ///  Henter den aktuelle bruger
        ///  Kun for autoriserede brugere
        ///  </summary>
        /// <returns>En UserReadDto med den aktuelle brugers detaljer</returns>
        /// <remarks>
        /// Dette endpoint henter information om den nuværende bruger ved at bruge ClaimTypes.NameIdentifier fra autorisationstokenet.
        /// Hvis brugerens ID ikke kan konverteres til int, returneres status 400 BadRequest.
        /// Hvis brugeren findes, returneres status 200 OK med information om brugeren.
        /// </remarks>
        [Authorize] // Этот endpoint доступен всем авторизованным пользователям
        [HttpGet("me")]
        public async Task<ActionResult<UserReadDto>> GetCurrentUser()
        {

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("Пользователь не найден в токене.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Неверный формат ID пользователя.");
                }

                var user = await _context.Users
                    .Include(u => u.Role) // У вас Role (единственное число), а не Roles
                    .Include(u => u.UserInfo)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound("Пользователь не найден в базе данных.");

                return Ok(new UserReadDto()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.UserInfo?.PhoneNumber,
                    Address = user.UserInfo?.Address,
                    City = user.UserInfo?.City,
                    ProfilePicture = user.UserInfo?.AvatarUrl ?? "https://www.manageengine.com/images/speaker-placeholder.png",
                    DateOfBirth = user.DateOfBirth,
                    isVIP = user.IsVIP
                });
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при получении текущего пользователя.");

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, $"Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже\n\n{ex.Message} {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Ændrer en brugers rolle
        /// Kun for autoriserede brugere med rolle Admin
        /// </summary>
        /// <param name="userId">ID af brugeren hvis rolle skal ændres</param>
        /// <param name="roleId">ID af den nye rolle</param>
        /// <returns>Returnerer NoContent hvis ændringen lykkedes</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)] // Только для администраторов
        [HttpPut("{userId}/role/{roleId}")]
        public async Task<IActionResult> ChangeUserRole(int userId, int roleId)
        {
            // Проверяем существование пользователя
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            // Проверяем существование роли
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                return NotFound("Роль не найдена.");
            }

            // Запрещаем назначать роль InfiniteVoid (только для Сатору Годжо)
            if (roleId == 5 && userId != 1)
            {
                return BadRequest("Роль InfiniteVoid может быть только у Сатору Годжо.");
            }

            // Изменяем роль пользователя
            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Роль пользователя {user.FirstName} {user.LastName} изменена на {role.Name}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при изменении роли пользователя.");
            }
        }

        /// <summary>
        /// Ændrer en brugers rolle (POST version)
        /// Kun for autoriserede brugere med rolle Admin
        /// </summary>
        /// <param name="roleUpdateDto">DTO med bruger ID og ny rolle ID</param>
        /// <returns>Returnerer besked om succes eller fejl</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRoleByDto([FromBody] UserRoleUpdateDto roleUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Проверяем существование пользователя
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == roleUpdateDto.UserId);
            
            if (user == null)
            {
                return NotFound("Пользователь не найден.");
            }

            // Проверяем существование роли
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleUpdateDto.RoleId);
            if (role == null)
            {
                return NotFound("Роль не найдена.");
            }

            // Запрещаем назначать роль InfiniteVoid кому-то кроме Сатору Годжо
            if (roleUpdateDto.RoleId == 5 && roleUpdateDto.UserId != 1)
            {
                return BadRequest("Роль InfiniteVoid может быть только у Сатору Годжо.");
            }

            var oldRoleName = user.Role.Name;
            
            // Изменяем роль пользователя
            user.RoleId = roleUpdateDto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new 
                { 
                    Message = $"Роль пользователя {user.FirstName} {user.LastName} изменена с '{oldRoleName}' на '{role.Name}'",
                    UserId = user.Id,
                    OldRole = oldRoleName,
                    NewRole = role.Name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при изменении роли пользователя.");
            }
        }

        /// <summary>
        /// Henter alle tilgængelige roller
        /// Kun for autoriserede brugere med rolle Admin eller Manager
        /// </summary>
        /// <returns>Liste af alle roller</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")]
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new { r.Id, r.Name })
                    .ToListAsync();

                return Ok(roles);
            }
            catch (Exception ex)
            {
                // Используем наш "журналист" (_logger)
                _logger.LogError(ex, "Ошибка при получении списка ролей.");

                // Возвращаем пользователю понятную ошибку
                return StatusCode(500, "Произошла внутренняя ошибка сервера. Пожалуйста, попробуйте позже.");
            }
        }
    }
}