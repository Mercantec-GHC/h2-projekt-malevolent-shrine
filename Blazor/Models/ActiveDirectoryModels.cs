namespace Blazor.Models
{
    public class ADUser
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public DateTime? LastLogon { get; set; }
        public DateTime? PasswordLastSet { get; set; }
        public bool IsEnabled { get; set; } = true;
        public List<string> Groups { get; set; } = new List<string>();
    }

    public class ADGroup
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new List<string>();
    }

    public class ConnectionTestResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
