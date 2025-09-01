using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    /// <summary>
    /// Controller for managing VIP rooms
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VipRoomsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialize VipRoomsController with database context
        /// </summary>
        /// <param name="context">Database context</param>
        public VipRoomsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all VIP rooms
        /// </summary>
        /// <returns>List of all VIP rooms</returns>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VipRoomReadDto>>> GetVipRooms()
        {
            try
            {
                var vipRooms = await _context.VipRooms
                    .Include(vr => vr.Hotel)
                    .ToListAsync();

                var result = vipRooms.Select(vr => new VipRoomReadDto
                {
                    Id = vr.Id,
                    Number = vr.Number,
                    Capacity = vr.Capacity,
                    PricePerNight = vr.PricePerNight,
                    Floor = vr.Floor,
                    IsAvailable = vr.IsAvailable,
                    HotelId = vr.HotelId,
                    VipServiceDescription = vr.VipServiceDescription,
                    ExtraAmenities = vr.ExtraAmenities,
                    Description = vr.Description,
                    ImageUrl = vr.ImageUrl,
                    CreatedAt = vr.CreatedAt,
                    UpdatedAt = vr.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a specific VIP room by ID
        /// </summary>
        /// <param name="id">VIP room ID</param>
        /// <returns>VIP room details</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VipRoomReadDto>> GetVipRoom(int id)
        {
            try
            {
                var vipRoom = await _context.VipRooms
                    .Include(vr => vr.Hotel)
                    .FirstOrDefaultAsync(vr => vr.Id == id);

                if (vipRoom == null)
                {
                    return NotFound($"VIP room with ID {id} not found.");
                }

                var result = new VipRoomReadDto
                {
                    Id = vipRoom.Id,
                    Number = vipRoom.Number,
                    Capacity = vipRoom.Capacity,
                    PricePerNight = vipRoom.PricePerNight,
                    Floor = vipRoom.Floor,
                    IsAvailable = vipRoom.IsAvailable,
                    HotelId = vipRoom.HotelId,
                    VipServiceDescription = vipRoom.VipServiceDescription,
                    ExtraAmenities = vipRoom.ExtraAmenities,
                    Description = vipRoom.Description,
                    ImageUrl = vipRoom.ImageUrl,
                    CreatedAt = vipRoom.CreatedAt,
                    UpdatedAt = vipRoom.UpdatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new VIP room
        /// </summary>
        /// <param name="vipRoomDto">VIP room creation data</param>
        /// <returns>Created VIP room</returns>
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpPost]
        public async Task<ActionResult<VipRoomReadDto>> CreateVipRoom(VipRoomCreateDto vipRoomDto)
        {
            try
            {
                // Check if hotel exists
                var hotel = await _context.Hotels.FindAsync(vipRoomDto.HotelId);
                if (hotel == null)
                {
                    return BadRequest($"Hotel with ID {vipRoomDto.HotelId} does not exist.");
                }

                // Check if room number already exists in this hotel
                var existingRoom = await _context.Rooms
                    .AnyAsync(r => r.Number == vipRoomDto.Number && r.HotelId == vipRoomDto.HotelId);

                if (existingRoom)
                {
                    return BadRequest($"Room number {vipRoomDto.Number} already exists in this hotel.");
                }

                var vipRoom = new VipRoom
                {
                    Number = vipRoomDto.Number,
                    Capacity = vipRoomDto.Capacity,
                    PricePerNight = vipRoomDto.PricePerNight,
                    Floor = vipRoomDto.Floor,
                    IsAvailable = vipRoomDto.IsAvailable,
                    HotelId = vipRoomDto.HotelId,
                    VipServiceDescription = vipRoomDto.VipServiceDescription,
                    ExtraAmenities = vipRoomDto.ExtraAmenities,
                    Description = vipRoomDto.Description,
                    ImageUrl = vipRoomDto.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.VipRooms.Add(vipRoom);
                await _context.SaveChangesAsync();

                var createdVipRoom = new VipRoomReadDto
                {
                    Id = vipRoom.Id,
                    Number = vipRoom.Number,
                    Capacity = vipRoom.Capacity,
                    PricePerNight = vipRoom.PricePerNight,
                    Floor = vipRoom.Floor,
                    IsAvailable = vipRoom.IsAvailable,
                    HotelId = vipRoom.HotelId,
                    VipServiceDescription = vipRoom.VipServiceDescription,
                    ExtraAmenities = vipRoom.ExtraAmenities,
                    Description = vipRoom.Description,
                    ImageUrl = vipRoom.ImageUrl,
                    CreatedAt = vipRoom.CreatedAt,
                    UpdatedAt = vipRoom.UpdatedAt
                };

                return CreatedAtAction(nameof(GetVipRoom), new { id = vipRoom.Id }, createdVipRoom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update an existing VIP room
        /// </summary>
        /// <param name="id">VIP room ID</param>
        /// <param name="vipRoomDto">Updated VIP room data</param>
        /// <returns>Updated VIP room</returns>
        ///
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpPut("{id}")]
        public async Task<ActionResult<VipRoomReadDto>> UpdateVipRoom(int id, VipRoomUpdateDto vipRoomDto)
        {
            try
            {
                if (id != vipRoomDto.Id)
                {
                    return BadRequest("ID mismatch.");
                }

                var vipRoom = await _context.VipRooms.FindAsync(id);
                if (vipRoom == null)
                {
                    return NotFound($"VIP room with ID {id} not found.");
                }

                // Check if hotel exists
                var hotel = await _context.Hotels.FindAsync(vipRoomDto.HotelId);
                if (hotel == null)
                {
                    return BadRequest($"Hotel with ID {vipRoomDto.HotelId} does not exist.");
                }

                // Check if room number already exists in this hotel (excluding current room)
                var existingRoom = await _context.Rooms
                    .AnyAsync(r => r.Number == vipRoomDto.Number && 
                                  r.HotelId == vipRoomDto.HotelId && 
                                  r.Id != id);

                if (existingRoom)
                {
                    return BadRequest($"Room number {vipRoomDto.Number} already exists in this hotel.");
                }

                // Update properties
                vipRoom.Number = vipRoomDto.Number;
                vipRoom.Capacity = vipRoomDto.Capacity;
                vipRoom.PricePerNight = vipRoomDto.PricePerNight;
                vipRoom.Floor = vipRoomDto.Floor;
                vipRoom.IsAvailable = vipRoomDto.IsAvailable;
                vipRoom.HotelId = vipRoomDto.HotelId;
                vipRoom.VipServiceDescription = vipRoomDto.VipServiceDescription;
                vipRoom.ExtraAmenities = vipRoomDto.ExtraAmenities;
                vipRoom.Description = vipRoomDto.Description;
                vipRoom.ImageUrl = vipRoomDto.ImageUrl;
                vipRoom.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedVipRoom = new VipRoomReadDto
                {
                    Id = vipRoom.Id,
                    Number = vipRoom.Number,
                    Capacity = vipRoom.Capacity,
                    PricePerNight = vipRoom.PricePerNight,
                    Floor = vipRoom.Floor,
                    IsAvailable = vipRoom.IsAvailable,
                    HotelId = vipRoom.HotelId,
                    VipServiceDescription = vipRoom.VipServiceDescription,
                    ExtraAmenities = vipRoom.ExtraAmenities,
                    Description = vipRoom.Description,
                    ImageUrl = vipRoom.ImageUrl,
                    CreatedAt = vipRoom.CreatedAt,
                    UpdatedAt = vipRoom.UpdatedAt
                };

                return Ok(updatedVipRoom);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete a VIP room
        /// </summary>
        /// <param name="id">VIP room ID</param>
        /// <returns>Success message</returns>
        ///
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVipRoom(int id)
        {
            try
            {
                var vipRoom = await _context.VipRooms.FindAsync(id);
                if (vipRoom == null)
                {
                    return NotFound($"VIP room with ID {id} not found.");
                }

                // Check if there are any active bookings for this room
                var hasActiveBookings = await _context.Bookings
                    .AnyAsync(b => b.RoomId == id && b.CheckOutDate > DateTime.UtcNow);

                if (hasActiveBookings)
                {
                    return BadRequest("Cannot delete VIP room with active bookings.");
                }

                _context.VipRooms.Remove(vipRoom);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"VIP room {vipRoom.Number} has been successfully deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get available VIP rooms for a specific date range
        /// </summary>
        /// <param name="checkIn">Check-in date</param>
        /// <param name="checkOut">Check-out date</param>
        /// <returns>List of available VIP rooms</returns>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<VipRoomReadDto>>> GetAvailableVipRooms(
            [FromQuery] DateTime checkIn, 
            [FromQuery] DateTime checkOut)
        {
            try
            {
                if (checkIn >= checkOut)
                {
                    return BadRequest("Check-in date must be before check-out date.");
                }

                if (checkIn < DateTime.UtcNow.Date)
                {
                    return BadRequest("Check-in date cannot be in the past.");
                }

                var availableVipRooms = await _context.VipRooms
                    .Include(vr => vr.Hotel)
                    .Where(vr => vr.IsAvailable && 
                                !_context.Bookings.Any(b => 
                                    b.RoomId == vr.Id && 
                                    b.CheckInDate < checkOut && 
                                    b.CheckOutDate > checkIn))
                    .ToListAsync();

                var result = availableVipRooms.Select(vr => new VipRoomReadDto
                {
                    Id = vr.Id,
                    Number = vr.Number,
                    Capacity = vr.Capacity,
                    PricePerNight = vr.PricePerNight,
                    Floor = vr.Floor,
                    IsAvailable = vr.IsAvailable,
                    HotelId = vr.HotelId,
                    VipServiceDescription = vr.VipServiceDescription,
                    ExtraAmenities = vr.ExtraAmenities,
                    Description = vr.Description,
                    ImageUrl = vr.ImageUrl,
                    CreatedAt = vr.CreatedAt,
                    UpdatedAt = vr.UpdatedAt
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
