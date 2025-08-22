using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Добавляем для авторизации

namespace API.Controllers
{
    /// <summary>
    /// Kontroller til håndtering af roller (læse, oprette, opdatere, slette).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af RolesController.
        /// </summary>
        /// <param name="context">Databasekontekst til håndtering af rolledata.</param>
        public RolesController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle roller.
        /// Kun personalet (Admin, Manager) og InfiniteVoid har adgang.
        /// </summary>
        /// <returns>En liste med alle roller.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Персонал и Годжо могут просматривать роли
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleReadDto>>> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();

            var roleReadDtos = roles.Select(r => new RoleReadDto
            {
                Id = r.Id,
                Name = r.Name,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            return Ok(roleReadDtos);
        }

        /// <summary>
        /// Henter oplysninger om en specifik rolle baseret på ID.
        /// Kun personalet (Admin, Manager) og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Rollens unikke ID.</param>
        /// <returns>Rollens oplysninger eller 404, hvis ikke fundet.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Персонал и Годжо могут просматривать конкретную роль
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleReadDto>> GetRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var roleReadDto = new RoleReadDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };

            return Ok(roleReadDto);
        }

        /// <summary>
        /// Opretter en ny rolle.
        /// Kun Admin og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="roleDto">DTO med oplysninger om den nye rolle.</param>
        /// <returns>Oplysninger om den oprettede rolle.</returns>
        [Authorize(Roles = "Admin,InfiniteVoid")] // Только админы и Годжо могут создавать роли
        [HttpPost]
        public async Task<ActionResult<RoleReadDto>> PostRole(RoleCreateDto roleDto)
        {
            var role = new Role
            {
                Name = roleDto.Name
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            var roleReadDto = new RoleReadDto
            {
                Id = role.Id,
                Name = role.Name,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            };

            return CreatedAtAction(nameof(GetRole), new { id = roleReadDto.Id }, roleReadDto);
        }

        /// <summary>
        /// Opdaterer en eksisterende rolle.
        /// Kun Admin og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Rollens unikke ID.</param>
        /// <param name="roleDto">DTO med opdaterede oplysninger.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,InfiniteVoid")] // Только админы и Годжо могут обновлять роли
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, RoleUpdateDto roleDto)
        {
            if (id != roleDto.Id)
            {
                return BadRequest();
            }

            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = roleDto.Name;
            role.UpdatedAt = DateTime.UtcNow;

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
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

        /// <summary>
        /// Sletter en rolle.
        /// Kun Admin og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Rollens unikke ID.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,InfiniteVoid")] // Только админы и Годжо могут удалять роли
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Kontrollerer, om en rolle findes i databasen.
        /// </summary>
        /// <param name="id">Rollens unikke ID.</param>
        /// <returns>True, hvis rollen findes; ellers false.</returns>
        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}