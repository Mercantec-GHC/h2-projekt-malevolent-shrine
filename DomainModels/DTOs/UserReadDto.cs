namespace DomainModels.DTOs
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Username { get; set; }
        public bool IsVIP { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}