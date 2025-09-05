using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class User : Common
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }
        
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string HashedPassword { get; set; }
        public byte[]? Salt { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public bool IsAdult => DateOfBirth <= DateTime.Today.AddYears(-18);

        
        //for VIP users
        public bool IsVIP { get; set; }
        
        //Adding Role
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public UserInfo? UserInfo { get; set; }
        
        public string? ProfilePicture { get; set; }

        private List<Booking> Bookings { get; set; } = new();
        
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        
    }
}