namespace API.Models
{
    public class User : Common
    {
        public string Email { get; set; }
        public string Username { get; set; }
        public byte[] HashedPassword { get; set; }
        public byte[]? Salt { get; set; }
        
        public DateTime DateOfBirth { get; set; }
        public bool IsAdult => DateOfBirth <= DateTime.Today.AddYears(-18);

        
        //for VIP users
        public bool IsVIP { get; set; }
        public int? VipRoomId { get; set; }
        public VipRoom VipRoom { get; set; }
        
        //Adding Role
        public int RoleId { get; set; }
        public Role Role { get; set; }
        
        public UserInfo? Info { get; set; } // 1:1 navigation
    }
}