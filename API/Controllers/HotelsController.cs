using Microsoft.AspNetCore.Mvc;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Добавляем для авторизации

namespace API.Controllers
{
    /// <summary>
    /// Kontroller til håndtering af hoteller (læse, oprette, opdatere, slette).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HotelsController : ControllerBase
    {
        private readonly AppDBContext _context;

        /// <summary>
        /// Initialiserer en ny instans af HotelsController.
        /// </summary>
        /// <param name="context">Databasekontekst til håndtering af hoteldata.</param>
        public HotelsController(AppDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Henter en liste over alle hoteller.
        /// Tilgængelig for alle brugere.
        /// </summary>
        /// <returns>En liste med alle hoteller.</returns>
        [HttpGet] // Чтение отелей доступно всем
        public async Task<ActionResult<IEnumerable<Hotel>>> GetHotels()
        {
            return await _context.Hotels.ToListAsync();
        }

        /// <summary>
        /// Henter oplysninger om et bestemt hotel baseret på dets ID.
        /// Tilgængelig for alle brugere.
        /// </summary>
        /// <param name="id">Hotellets unikke ID.</param>
        /// <returns>Hotellets oplysninger eller 404, hvis ikke fundet.</returns>
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

        /// <summary>
        /// Opretter et nyt hotel.
        /// Kun Admin, Manager og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="hotelDto">DTO med oplysninger om det nye hotel.</param>
        /// <returns>Oplysninger om det oprettede hotel.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Только админы и Годжо могут создавать отели
        [HttpPost]
        public async Task<ActionResult<HotelReadDto>> PostHotel(HotelCreateDto hotelDto)
        {
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

        /// <summary>
        /// Opdaterer et eksisterende hotel.
        /// Kun Admin, Manager og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Hotellets unikke ID.</param>
        /// <param name="hotelDto">DTO med opdaterede oplysninger.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Только админы и Годжо могут обновлять отели
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHotel(int id, HotelUpdateDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = new Hotel
            {
                Id = hotelDto.Id,
                Name = hotelDto.Name,
                Address = hotelDto.Address,
               
            };

            _context.Entry(hotel).State = EntityState.Modified;

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

        /// <summary>
        /// Sletter et hotel.
        /// Kun Admin, Manager og InfiniteVoid har adgang.
        /// </summary>
        /// <param name="id">Hotellets unikke ID.</param>
        /// <returns>NoContent ved succes, ellers passende fejlkode.</returns>
        [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Только админы и Годжо могут удалять отели
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