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

            //  чтобы он сам ставил текущее время, HasDefaultValueSql - автоматически заполняет поле
            // текущей датой и временем при добавлении новой записи
            modelBuilder.Entity<Role>().Property(r => r.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Role>().Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
            
            modelBuilder.Entity<UserInfo>()
                .HasKey(i => i.UserId); // Shared PK

            modelBuilder.Entity<User>()
                .HasOne(u => u.Info)
                .WithOne(i => i.User)
                .HasForeignKey<UserInfo>(i => i.UserId);
        }
    }
}