using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Добавляем для авторизации

namespace API.Controllers
{
    /// <summary>
    /// Kontroller til håndtering af værelser (læse, oprette, opdatere, slette).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af RoomsController.
        /// </summary>
        /// <param name="context">Databasekontekst til håndtering af værelsesdata.</param>
        public RoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter alle værelser.
        /// Tilgængelig for alle brugere.
        /// </summary>
        /// <returns>En liste over alle værelser.</returns>
        [HttpGet] // Чтение комнат доступно всем
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        /// <summary>
        /// Henter oplysninger om et specifikt værelse baseret på ID.
        /// Tilgængelig for alle brugere.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <returns>Værelsets oplysninger eller 404, hvis ikke fundet.</returns>
        [HttpGet("{id}")] // Чтение конкретной комнаты доступно всем
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        /// <summary>
        /// Opretter et nyt værelse.
        /// Kun Admin, Manager, Receptionist og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="roomDto">DTO med oplysninger om det nye værelse.</param>
        /// <returns>Oplysninger om det oprettede værelse.</returns>
        [Authorize(Roles = "Admin,Manager,Receptionist,InfiniteVoid")] // Админы, менеджеры, ресепшн и Годжо могут создавать комнаты
        [HttpPost]
        public async Task<ActionResult<RoomReadDto>> PostRoom(RoomCreateDto roomDto)
        {
            var room = new Room
            {
                Number = roomDto.Number,
                Capacity = roomDto.Capacity,
                PricePerNight = roomDto.PricePerNight,
                Floor = roomDto.Floor,
                IsAvailable = roomDto.IsAvailable,
                HotelId = roomDto.HotelId
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var roomReadDto = new RoomReadDto
            {
                Id = room.Id,
                Number = room.Number,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                Floor = room.Floor,
                IsAvailable = room.IsAvailable,
                HotelId = room.HotelId
            };

            return CreatedAtAction(nameof(GetRoom), new { id = roomReadDto.Id }, roomReadDto);
        }

        /// <summary>
        /// Opdaterer et eksisterende værelse.
        /// Kun Admin, Manager, Receptionist og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <param name="roomDto">DTO med opdaterede oplysninger.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,Manager,Receptionist,InfiniteVoid")] // Админы, менеджеры, ресепшн и Годжо могут обновлять комнаты
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, RoomUpdateDto roomDto)
        {
            if (id != roomDto.Id)
            {
                return BadRequest();
            }

            var room = new Room
            {
                Id = roomDto.Id,
                Number = roomDto.Number,
                Capacity = roomDto.Capacity,
                PricePerNight = roomDto.PricePerNight,
                Floor = roomDto.Floor,
                IsAvailable = roomDto.IsAvailable,
                HotelId = roomDto.HotelId
            };
            
            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
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
        /// Kontrollerer, om et værelse findes i databasen.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <returns>True, hvis værelset findes; ellers false.</returns>
        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        /// <summary>
        /// Sletter et værelse.
        /// Kun Admin, Manager og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Værelsets unikke ID.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Только админы, менеджеры и Годжо могут удалять комнаты
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}