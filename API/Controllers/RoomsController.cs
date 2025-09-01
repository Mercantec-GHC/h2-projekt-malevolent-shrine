using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Добавляем для авторизации

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public RoomsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet] // Чтение комнат доступно всем
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }
        
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
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Админы, менеджеры, ресепшн и Годжо могут создавать комнаты
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
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Админы, менеджеры, ресепшн и Годжо могут обновлять комнаты
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
        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Только админы, менеджеры и Годжо могут удалять комнаты
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