using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;
using Microsoft.AspNetCore.Identity; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public class DataSeederService
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DataSeederService> _logger;
        private readonly PasswordHasher<User> _passwordHasher;

        public DataSeederService(AppDBContext context, ILogger<DataSeederService> logger, PasswordHasher<User> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        public async Task<List<User>> SeedUsersAsync(int count)
        {
            var faker = new Faker<User>("en")
                .RuleFor(u => u.Email, f => f.Internet.Email().ToLower())
                .RuleFor(u => u.HashedPassword, (f, u) => _passwordHasher.HashPassword(u, "Password123!"))
                .RuleFor(u => u.RoleId, f => f.Random.ListItem(_context.Roles.ToList()).Id)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(1))
                .RuleFor(u => u.UpdatedAt, (f, u) => u.CreatedAt.AddMinutes(f.Random.Int(1, 60)))
                .RuleFor(u => u.Username,
                    (f, u) =>
                        $"{u.FirstName.ToLower()}.{u.LastName.ToLower()}_{f.Random.Int(1000, 9999)}") // Уникальное имя пользователя
                .RuleFor(u => u.UserInfo, f => new UserInfo
                {
                    PhoneNumber = f.Phone.PhoneNumber(),
                    Address = f.Address.FullAddress()
                });
    
            var users = faker.Generate(count);
            
            _context.Users.AddRange(users);
            await _context.SaveChangesAsync();
            
    
            return users;
        }
        
        public async Task<List<Hotel>> SeedHotelsAsync(int count)
        {
            var hotels = new Faker<Hotel>("en")
                .RuleFor(h => h.Name, f => f.Company.CompanyName())
                .RuleFor(h => h.Address, f => f.Address.FullAddress())
                .RuleFor(h => h.CreatedAt, f => f.Date.Past(1))
                .RuleFor(h => h.UpdatedAt, (f, h) => h.CreatedAt.AddMinutes(f.Random.Int(1, 60)))
                .RuleFor(h => h.Rooms, new List<Room>()) // Изначально комнаты пустые
                .Generate(count);

            // Здесь мы добавим код для создания комнат...
            foreach (var hotel in hotels)
            {
                // Здесь мы создадим обычные комнаты
                // Здесь мы создадим обычные комнаты
                var rooms = new Faker<Room>("en") // Добавьте "en" здесь
                    .RuleFor(r => r.Number, f => f.Random.Int(100, 999).ToString()) // Случайный номер комнаты
                    .RuleFor(r => r.PricePerNight, f => f.Random.Decimal(50, 500)) // Случайная цена
                    .RuleFor(r => r.Capacity, f => f.Random.Int(1, 4)) // Случайная вместимость
                    .RuleFor(r => r.Floor, f => f.Random.Int(1, 10)) // Случайный этаж
                    .RuleFor(r => r.IsAvailable, f => f.Random.Bool()) // Случайно доступна или нет
                    .RuleFor(r => r.HotelId, hotel.Id) // Связываем с отелем
                    .RuleFor(r => r.CreatedAt, f => f.Date.Past(1))
                    .RuleFor(r => r.UpdatedAt, (f, r) => r.CreatedAt.AddMinutes(f.Random.Int(1, 60)))
                    .Generate(10);
                
                // Здесь мы создадим VIP комнаты
                var vipRooms = new Faker<VipRoom>("en")
                    .RuleFor(vr => vr.Number, f => f.Random.Int(1000, 1099).ToString())
                    .RuleFor(vr => vr.PricePerNight, f => f.Random.Decimal(500, 1500))
                    .RuleFor(vr => vr.Capacity, f => f.Random.Int(2, 6))
                    .RuleFor(vr => vr.Floor, f => f.Random.Int(8, 10))
                    .RuleFor(vr => vr.IsAvailable, f => f.Random.Bool())
                    .RuleFor(vr => vr.HotelId, hotel.Id)
                    .RuleFor(vr => vr.CreatedAt, f => f.Date.Past(1))
                    .RuleFor(vr => vr.UpdatedAt, (f, vr) => vr.CreatedAt.AddMinutes(f.Random.Int(1, 60)))
                    // Добавляем дополнительные удобства для VIP комнат
                    .RuleFor(vr => vr.ExtraAmenities, f => f.Random.ListItems(new List<string> { "Hello Kitty", "Kuromi", "Deadpool" }, 3)) // Пример удобств
                    .RuleFor(vr => vr.Description, f => f.Lorem.Sentence(10)) // Описание комнаты
                    .Generate(2);

                // Теперь мы добавим их в отель
                hotel.Rooms.AddRange(rooms);
                hotel.Rooms.AddRange(vipRooms);
            }

            // Здесь мы добавим код для сохранения в базу...
            _context.Hotels.AddRange(hotels);
            await _context.SaveChangesAsync();

            return hotels;
        }
        
        public async Task ClearAllDataAsync()
        {
            _context.Bookings.RemoveRange(_context.Bookings);
            _context.Rooms.RemoveRange(_context.Rooms);
            _context.Hotels.RemoveRange(_context.Hotels);
            _context.Users.RemoveRange(_context.Users);
            await _context.SaveChangesAsync();
            _logger.LogInformation("All data cleared from Bookings, Rooms, Hotels, and Users tables.");
        }
    }
}