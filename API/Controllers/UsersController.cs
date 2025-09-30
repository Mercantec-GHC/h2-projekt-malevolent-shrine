using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Services;
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;


namespace API.Controllers
{
    /// <summary>
    /// User management endpoints (CRUD, role updates, and current user lookup).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<UsersController> _logger;
        
        
        /// <summary>
        /// Initializes a new instance of <see cref="UsersController"/>.
        /// </summary>
        /// <param name="context">Application database context.</param>
        /// <param name="jwtService">JWT issuance service.</param>
        /// <param name="logger">Logger for this controller.</param>
        public UsersController(AppDBContext context, JwtService jwtService, ILogger<UsersController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }
        
       /// <summary>
       /// Returns all users.
       /// Requires authorization with appropriate roles.
       /// </summary>
       /// <returns>List of users mapped to <see cref="UserReadDto"/>.</returns>
      
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserInfo)
                    .ToListAsync();

                var userReadDtos = users.Select(u => new UserReadDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.UserInfo?.PhoneNumber ?? "",
                    Address = u.UserInfo?.Address ?? ""
                }).ToList();

                return Ok(userReadDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving users.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
            
        }

        /// <summary>
        /// Returns details of a specific user by id.
        /// </summary>
        /// <param name="id">Unique user identifier.</param>
        /// <returns><see cref="UserReadDto"/> if found; 404 otherwise.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] 
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserInfo)
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
                    PhoneNumber = user.UserInfo?.PhoneNumber ?? "",
                    Address = user.UserInfo?.Address ?? ""
                };

                return Ok(userReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving user with ID {UserId}.", id);
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Creates a new user.
        /// Admin/Manager/InfiniteVoid only.
        /// </summary>
        /// <param name="userDto">Payload to create a new user.</param>
        /// <returns>Created <see cref="UserReadDto"/> with route to <see cref="GetUser"/>.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> PostUser(UserCreateDto userDto)

        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("A user with this email already exists.");

            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return BadRequest("A user with this username already exists.");

            
            try
            {
                var user = new User
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    Email = userDto.Email,
                    Username = userDto.Username,
                    HashedPassword = hashedPassword,
                    RoleId = 4,
                    UserInfo = new UserInfo
                    {
                        PhoneNumber = userDto.PhoneNumber,
                        Address = userDto.Address
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
                    PhoneNumber = user.UserInfo.PhoneNumber,
                    Address = user.UserInfo.Address
                };
                return CreatedAtAction(nameof(GetUser), new { id = userReadDto.Id }, userReadDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating user.");
                return StatusCode(500, $"An internal server error occurred. Please try again later. {ex.Message}\n\n{ex}");
            }
        }
        
        /// <summary>
        /// Updates an existing user.
        /// Admin/Manager/InfiniteVoid only.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <param name="userDto">Updated user data.</param>
        /// <returns>No content on success; 404 if not found.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
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
            
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != id))
                return BadRequest("A user with this email already exists.");
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username && u.Id != id))
                return BadRequest("A user with this username already exists.");


            var user = await _context.Users
                .Include(u => u.UserInfo)
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
                user.UserInfo = new UserInfo();
            }

            user.UserInfo.PhoneNumber = userDto.PhoneNumber;
            user.UserInfo.Address = userDto.Address;
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
        /// Deletes an existing user.
        /// Admin/Manager/InfiniteVoid only.
        /// </summary>
        /// <param name="id">User identifier.</param>
        /// <returns>No content on success; 404 if not found.</returns>
      
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
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
                _logger.LogError(ex, "Error while deleting user.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
            
        }

        /// <summary>
        /// Returns the currently authenticated user profile.
        /// </summary>
        /// <returns>Basic user data including role name.</returns>
        /// <remarks>
        /// Uses <see cref="ClaimTypes.NameIdentifier"/> to resolve the current user id from the access token.
        /// </remarks>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<User>> GetCurrentUser()
        {

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim == null)
                {
                    return Unauthorized("User id not found in token.");
                }

                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Invalid user id format.");
                }

                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.UserInfo)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return NotFound("User not found.");

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.IsVIP,
                    user.LastName,
                    user.DateOfBirth,
                    user.CreatedAt,
                    user.Role,
                    user.ProfilePicture
                });  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving current user.");
                return StatusCode(500, $"An internal server error occurred. Please try again later\n\n{ex.Message} {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Changes a user's role by ids.
        /// Admin/InfiniteVoid only.
        /// </summary>
        /// <param name="userId">Target user id.</param>
        /// <param name="roleId">New role id to assign.</param>
        /// <returns>200 OK with a message, 404 if user/role not found.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)]
        [HttpPut("{userId}/role/{roleId}")]
        public async Task<IActionResult> ChangeUserRole(int userId, int roleId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            if (roleId == 5 && userId != 1)
            {
                return BadRequest("Role 'InfiniteVoid' can be assigned to a specific user only.");
            }

            user.RoleId = roleId;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Role for user {user.FirstName} {user.LastName} changed to {role.Name}" });
            }
            catch (Exception)
            {
                return StatusCode(500, "Error while changing user's role.");
            }
        }

        /// <summary>
        /// Changes a user's role using a DTO payload.
        /// Admin/InfiniteVoid only.
        /// </summary>
        /// <param name="roleUpdateDto">DTO with user id and new role id.</param>
        /// <returns>200 OK with details, 404 if user/role not found.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRoleByDto([FromBody] UserRoleUpdateDto roleUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == roleUpdateDto.UserId);
            
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleUpdateDto.RoleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            if (roleUpdateDto.RoleId == 5 && roleUpdateDto.UserId != 1)
            {
                return BadRequest("Role 'InfiniteVoid' can be assigned to a specific user only.");
            }

            var oldRoleName = user.Role.Name;
            
            user.RoleId = roleUpdateDto.RoleId;
            user.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new 
                { 
                    Message = $"Role for user {user.FirstName} {user.LastName} changed from '{oldRoleName}' to '{role.Name}'",
                    UserId = user.Id,
                    OldRole = oldRoleName,
                    NewRole = role.Name
                });
            }
            catch (Exception)
            {
                return StatusCode(500, "Error while changing user's role.");
            }
        }

        /// <summary>
        /// Returns all available roles (id and name only).
        /// </summary>
        /// <returns>List of roles.</returns>
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
                _logger.LogError(ex, "Error while retrieving roles.");
                return StatusCode(500, "An internal server error occurred. Please try again later.");
            }
        }

        /// <summary>
        /// Returns users filtered by role name.
        /// </summary>
        /// <param name="roleName">Role name to filter by (e.g., 'Rengøring').</param>
        /// <returns>List of <see cref="UserReadDto"/> for users in the given role.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpGet("by-role/{roleName}")]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsersByRole(string roleName)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.UserInfo)
                    .Where(u => u.Role.Name == roleName)
                    .ToListAsync();

                var result = users.Select(u => new UserReadDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.UserInfo?.PhoneNumber ?? string.Empty,
                    Address = u.UserInfo?.Address ?? string.Empty
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while retrieving users by role {Role}", roleName);
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}
