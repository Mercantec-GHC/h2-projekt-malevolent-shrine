using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<VipRoom> VipRooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Статическая дата для seed данных
            var seedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // настраиваем связь между User и Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // добавить эти роли в базу данных
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Role { Id = 2, Name = "Receptionist", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Role { Id = 3, Name = "Rengøring", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Role { Id = 4, Name = "Kunde", CreatedAt = seedDate, UpdatedAt = seedDate },
                new Role { Id = 5, Name = "InfiniteVoid", CreatedAt = seedDate, UpdatedAt = seedDate }
            );
            
            // Наш Сатору Годжо как супер-админ
            modelBuilder.Entity<User>().HasData(
                new User 
                { 
                    Id = 1, 
                    FirstName = "Satoru", 
                    LastName = "Gojo",
                    Username = "superadmin",
                    Email = "admin@system.com",
                    HashedPassword = "$2a$11$hashed_password_here", // строка вместо реального хеша
                    RoleId = 5, // InfiniteVoid роль
                    CreatedAt = seedDate, 
                    UpdatedAt = seedDate,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsVIP = true
                }
            );

            //  чтобы он сам ставил текущее время, HasDefaultValueSql - автоматически заполняет поле
            // текущей датой и временем при добавлении новой записи
            modelBuilder.Entity<Role>().Property(r => r.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Role>().Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
            
            modelBuilder.Entity<UserInfo>()
                .HasKey(i => i.UserId); // Shared PK

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserInfo)
                .WithOne(i => i.User)
                .HasForeignKey<UserInfo>(i => i.UserId);
        }
    }
}