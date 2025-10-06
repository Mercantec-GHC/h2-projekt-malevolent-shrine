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
    public class HotelsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public HotelsController(AppDBContext context)
        {
            _context = context;
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<HotelReadDto>>> GetHotels()
        {
            var hotels = await _context.Hotels
                .Include(h => h.Rooms)
                .Select(h => new HotelReadDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    ImageUrl = h.ImageUrl,
                    Rooms = h.Rooms.Select(r => new RoomReadDto
                    {
                        Id = r.Id,
                        Number = r.Number,
                        Capacity = r.Capacity,
                        PricePerNight = r.PricePerNight,
                        Floor = r.Floor,
                        IsAvailable = r.IsAvailable,
                        HotelId = r.HotelId,
                        ImageUrl = r.ImageUrl,
                        Description = r.Description
                    }).ToList()
                })
                .ToListAsync();

            return Ok(hotels);
        }
        
        [HttpGet("{id}")] // Чтение конкретного отеля доступно всем
        public async Task<ActionResult<Hotel>> GetHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);

            if (hotel == null)
            {
                return NotFound(); // Возвращаем 404, если отель не найден
            }

            return hotel; // Возвращаем отель
        }
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager + "," + RoleNames.InfiniteVoid)] // Только админы и Годжо могут создавать отели
        [HttpPost]
        public async Task<ActionResult<HotelReadDto>> PostHotel(HotelCreateDto hotelDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var hotel = new Hotel
            {
                Name = hotelDto.Name,
                Address = hotelDto.Address
            };

            _context.Hotels.Add(hotel);
            await _context.SaveChangesAsync();

            var hotelReadDto = new HotelReadDto
            {
                Id = hotel.Id,
                Name = hotel.Name,
                Address = hotel.Address
            };

            return CreatedAtAction(nameof(GetHotel), new { id = hotelReadDto.Id }, hotelReadDto);
        }
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)] // Только админы и Годжо могут обновлять отели
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, HotelUpdateDto hotelDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            hotel.Name = hotelDto.Name;
            hotel.Address = hotelDto.Address;
            hotel.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HotelExists(id))
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
        private bool HotelExists(int id)
        {
            return _context.Hotels.Any(e => e.Id == id);
        }
        
        [Authorize(Roles = RoleNames.Admin + "," + RoleNames.InfiniteVoid)] // Только админы и Годжо могут удалять отели
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _context.Hotels.FindAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            _context.Hotels.Remove(hotel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}