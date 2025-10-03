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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.GetBaseException().Message;
                if (msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) || msg.Contains("23505"))
                {
                    return Conflict("Room number must be unique within the hotel.");
                }
                throw;
            }

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
            room.UpdatedAt = DateTime.UtcNow;
            
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
            catch (DbUpdateException ex)
            {
                var msg = ex.GetBaseException().Message;
                if (msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) || msg.Contains("23505"))
                {
                    return Conflict("Room number must be unique within the hotel.");
                }
                throw;
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

        /// <summary>
        /// Returns availability for rooms for the next N days. Optionally filter by hotel.
        /// </summary>
        /// <param name="days">Number of days to include (default 28, max 60).</param>
        /// <param name="hotelId">Optional hotel id to filter rooms.</param>
        [AllowAnonymous]
        [HttpGet("availability")]
        public async Task<ActionResult<List<RoomAvailabilityDto>>> GetAvailability([FromQuery] int days = 28, [FromQuery] int? hotelId = null)
        {
            if (days <= 0) days = 28;
            if (days > 60) days = 60;
            var start = DateTime.UtcNow.Date;
            var end = start.AddDays(days);

            var roomsQuery = _context.Rooms.AsNoTracking();
            if (hotelId.HasValue)
                roomsQuery = roomsQuery.Where(r => r.HotelId == hotelId);
            var rooms = await roomsQuery
                .Select(r => new { r.Id, r.Number, r.HotelId })
                .ToListAsync();
            var roomIds = rooms.Select(r => r.Id).ToList();
            if (roomIds.Count == 0)
                return Ok(new List<RoomAvailabilityDto>());

            var bookings = await _context.Bookings.AsNoTracking()
                .Where(b => roomIds.Contains(b.RoomId)
                            && b.CheckInDate < end
                            && b.CheckOutDate > start)
                .Select(b => new { b.Id, b.RoomId, b.CheckInDate, b.CheckOutDate })
                .ToListAsync();

            var bookingsByRoom = bookings.GroupBy(b => b.RoomId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<RoomAvailabilityDto>(rooms.Count);
            foreach (var r in rooms)
            {
                var dto = new RoomAvailabilityDto
                {
                    RoomId = r.Id,
                    RoomNumber = r.Number,
                    HotelId = r.HotelId
                };

                bookingsByRoom.TryGetValue(r.Id, out var roomBookings);

                for (int i = 0; i < days; i++)
                {
                    var d = start.AddDays(i);
                    // Занято, если d ∈ [CheckInDate, CheckOutDate)
                    var hit = roomBookings?.FirstOrDefault(b => b.CheckInDate.Date <= d && b.CheckOutDate.Date > d);
                    dto.Days.Add(new AvailabilityDayDto
                    {
                        Date = d,
                        IsOccupied = hit != null,
                        BookingId = hit?.Id
                    });
                }
                result.Add(dto);
            }

            return Ok(result);
        }
    }
}