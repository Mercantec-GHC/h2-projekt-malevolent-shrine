using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using API.DTOs;
using Microsoft.AspNetCore.Authorization; // Добавляем для авторизации

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly AppDBContext _context;

    public BookingsController(AppDBContext context)
    {
        _context = context;
    }

    // GET: api/bookings
    [Authorize(Roles = "Admin,Manager,Receptionist,InfiniteVoid")] // Только персонал и Годжо могут видеть все бронирования
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingReadDto>>> GetBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .ThenInclude(r => r.Hotel)
            .ToListAsync();

        var bookingReadDtos = bookings.Select(b => new BookingReadDto
        {
            Id = b.Id,
            UserId = b.UserId,
            RoomId = b.RoomId,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        }).ToList();

        return Ok(bookingReadDtos);
    }

    // GET: api/bookings/5
    [Authorize(Roles = "Admin,Manager,Receptionist,InfiniteVoid")] // Только персонал и Годжо могут видеть конкретное бронирование
    [HttpGet("{id}")]
    public async Task<ActionResult<BookingReadDto>> GetBooking(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Room)
            .ThenInclude(r => r.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return NotFound();
        }

        var bookingReadDto = new BookingReadDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            RoomId = booking.RoomId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };

        return Ok(bookingReadDto);
    }

    // POST: api/bookings
    [Authorize] // Любой авторизованный пользователь может создавать бронирования
    [HttpPost]
    public async Task<ActionResult<BookingReadDto>> CreateBooking(BookingCreateDto bookingCreateDto)
    {
        var booking = new Booking
        {
            UserId = bookingCreateDto.UserId,
            RoomId = bookingCreateDto.RoomId,
            CheckInDate = bookingCreateDto.CheckInDate,
            CheckOutDate = bookingCreateDto.CheckOutDate,
            TotalPrice = bookingCreateDto.TotalPrice,
            Status = bookingCreateDto.Status
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        var bookingReadDto = new BookingReadDto
        {
            Id = booking.Id,
            UserId = booking.UserId,
            RoomId = booking.RoomId,
            CheckInDate = booking.CheckInDate,
            CheckOutDate = booking.CheckOutDate,
            TotalPrice = booking.TotalPrice,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt
        };

        return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, bookingReadDto);
    }

    // PUT: api/bookings/5
    [Authorize(Roles = "Admin,Manager,Receptionist,InfiniteVoid")] // Только персонал и Годжо могут обновлять бронирования
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(int id, BookingUpdateDto bookingUpdateDto)
    {
        if (id != bookingUpdateDto.Id)
        {
            return BadRequest();
        }

        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        booking.UserId = bookingUpdateDto.UserId;
        booking.RoomId = bookingUpdateDto.RoomId;
        booking.CheckInDate = bookingUpdateDto.CheckInDate;
        booking.CheckOutDate = bookingUpdateDto.CheckOutDate;
        booking.TotalPrice = bookingUpdateDto.TotalPrice;
        booking.Status = bookingUpdateDto.Status;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookingExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/bookings/5
    [Authorize(Roles = "Admin,Manager,InfiniteVoid")] // Только админы, менеджеры и Годжо могут удалять бронирования
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.Id == id);
    }
}