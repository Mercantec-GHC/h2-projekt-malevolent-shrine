using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;



namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly PasswordHasher<User> _passwordHasher;
       

        public UsersController(AppDBContext context, JwtService jwtService, PasswordHasher<User> passwordHasher)
        {
            _context = context;
            _jwtService = jwtService;
            _passwordHasher = passwordHasher;
        } 
        
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
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

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto userDto)

        {
            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdateDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest();
            }

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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
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
        
        
    }
    
}