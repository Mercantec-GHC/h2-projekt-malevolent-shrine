namespace DomainModels.Models;

public class UserInfo
{
        public int UserId { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? AvatarUrl { get; set; }

        public User User { get; set; } = default!;
}