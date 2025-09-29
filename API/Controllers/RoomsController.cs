using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    /// <summary>
    /// Endpoints for managing rooms.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="RoomsController"/>.
        /// </summary>
        public RoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns all rooms.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }
        
        /// <summary>
        /// Returns a specific room by id.
        /// </summary>
        /// <param name="id">Room identifier.</param>
        [HttpGet("{id}")]
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
        /// Creates a new room.
        /// </summary>
        /// <param name="roomDto">Room creation payload.</param>
        /// <returns>Created room as <see cref="RoomReadDto"/>.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
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
        /// Updates an existing room.
        /// </summary>
        /// <param name="id">Room identifier.</param>
        /// <param name="roomDto">Room update payload.</param>
        /// <returns>No content on success.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, RoomUpdateDto roomDto)
        {
            if (id != roomDto.Id)
            {
                return BadRequest();
            }
            
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            
            room.Number = roomDto.Number;
            room.Capacity = roomDto.Capacity;
            room.PricePerNight = roomDto.PricePerNight;
            room.Floor = roomDto.Floor;
            room.IsAvailable = roomDto.IsAvailable;
            room.HotelId = roomDto.HotelId;
            
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
        
        /// <summary>
        /// Deletes a room by id.
        /// </summary>
        /// <param name="id">Room identifier.</param>
        /// <returns>No content on success.</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
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