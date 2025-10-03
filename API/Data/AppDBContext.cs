using Microsoft.EntityFrameworkCore;
using API.Models;


namespace API.Data
{
    /// <summary>
    /// Databasekontekst for applikationen — entrypoint for EF Core.
    /// Indeholder DbSet'er for brugere, roller, hoteller, værelser og bookinger.
    /// </summary>
    public class AppDBContext : DbContext
    {
        
        /// <summary>
        /// Opretter en ny instans af AppDBContext med de givne options.
        /// </summary>
        /// <param name="options">DbContext-indstillinger leveret via DI (Program.cs).</param>
        
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        /// <summary>
        /// Samling af brugere (tabellen Users).
        /// </summary>
        public DbSet<User> Users { get; set; }
        
        /// <summary>
        /// Samling af brugerinfo (tabellen UserInfos).
        ///  </summary>
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        /// <summary>
        /// Samling af roller (tabellen Roles).
        /// </summary>
        public DbSet<Role> Roles { get; set; }
        
        /// <summary>
        /// Samling af hoteller (tabellen Hotels).
        /// </summary>
        public DbSet<Hotel> Hotels { get; set; }
        
        /// <summary>
        /// Samling af værelser (tabellen Rooms).
        /// </summary>
        public DbSet<Room> Rooms { get; set; }
        
        /// <summary>
        /// Samling af VIP-værelser (tabellen VipRooms).
        /// </summary>
        public DbSet<VipRoom> VipRooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<UserInfo> UserInfos { get; set; }
        public DbSet<CleaningTask> CleaningTasks { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketMessage> TicketMessages { get; set; }
        /// <summary>
        /// Konfigurerer konteksten ved opstart.
        /// Her sætter vi kompatibilitet for Npgsql-tidsstempler.
        /// </summary>
        /// <param name="optionsBuilder">Options builder til DbContext.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        
        /// <summary>
        /// Konfigurerer EF Core-modellen: relationer, seed-data og standardværdier.
        /// Her sættes relationer mellem User ↔ Role og User ↔ UserInfo samt seed-data.
        /// </summary>
        /// <param name="modelBuilder">ModelBuilder til at definere entiteter og relationer.</param>
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
                new Role { Id = 2, Name = "Manager", CreatedAt = seedDate, UpdatedAt = seedDate },
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
                    HashedPassword = "$2a$11$XvW7nt9K.oF9nK7nMGqXnOHzCzEEwQm6HZJV8KfODaI3kJWkQkQaK", // Статический хеш для "SuperSecretPassword123!"
                    RoleId = 5, // InfiniteVoid роль
                    CreatedAt = seedDate, 
                    UpdatedAt = seedDate,
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsVIP = true
                }
            );
            
            // Устанавливаем текущую дату по умолчанию для CreatedAt и UpdatedAt
            modelBuilder.Entity<Role>().Property(r => r.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Role>().Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<User>().Property(u => u.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Booking>().Property(b => b.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Booking>().Property(b => b.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Hotel>().Property(h => h.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Hotel>().Property(h => h.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Room>().Property(r => r.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Room>().Property(r => r.UpdatedAt).HasDefaultValueSql("now()");
modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<Room>().HasIndex(r => new { r.HotelId, r.Number }).IsUnique();
            
            // Конфигурация CleaningTask
            modelBuilder.Entity<CleaningTask>()
                .HasOne(ct => ct.Room)
                .WithMany()
                .HasForeignKey(ct => ct.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<CleaningTask>()
                .HasOne(ct => ct.AssignedToUser)
                .WithMany()
                .HasForeignKey(ct => ct.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CleaningTask>()
                .HasOne(ct => ct.CreatedByUser)
                .WithMany()
                .HasForeignKey(ct => ct.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<CleaningTask>().Property(ct => ct.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<CleaningTask>().Property(ct => ct.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<CleaningTask>().HasIndex(ct => new { ct.AssignedToUserId, ct.Status });
            
            modelBuilder.Entity<UserInfo>()
                .HasKey(i => i.UserId); // Shared PK

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserInfo)
                .WithOne(i => i.User)
                .HasForeignKey<UserInfo>(i => i.UserId);
                
            // Конфигурация для VipRoom - создание отдельной таблицы (Table-per-Type)
            modelBuilder.Entity<VipRoom>()
                .ToTable("VipRooms") // Создаем отдельную таблицу VipRooms
                .Property(vr => vr.ExtraAmenities)
                .HasConversion(
                    v => string.Join(',', v), // Преобразование List<string> в строку для БД
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() // Обратное преобразование
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2), // Сравнение списков
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), // Вычисление хеша
                    c => c.ToList() // Создание снимка
                ));
           
            
            // Добавить в конец метода OnModelCreating:
            modelBuilder.Entity<RefreshToken>().HasIndex(rt => rt.TokenHash).IsUnique();
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<RefreshToken>().Property(rt => rt.UpdatedAt).HasDefaultValueSql("now()");
            
            // Ticket relations
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Ticket>()
                .Property(t => t.UpdatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.CreatedByUserId, t.Status });
            modelBuilder.Entity<Ticket>()
                .HasIndex(t => new { t.TargetRoleName, t.Status });

            // TicketMessage relations
            modelBuilder.Entity<TicketMessage>()
                .HasOne(m => m.Ticket)
                .WithMany(t => t.Messages)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TicketMessage>()
                .HasOne(m => m.SenderUser)
                .WithMany()
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TicketMessage>()
                .Property(m => m.CreatedAt).HasDefaultValueSql("now()");
            modelBuilder.Entity<TicketMessage>()
                .Property(m => m.UpdatedAt).HasDefaultValueSql("now()");
        }
    }
}