namespace API.Services
{
    public class AdRoleMappingOptions
    {
        // Use case-insensitive keys so 'hotel_admins' and 'Hotel_Admins' both match
        public Dictionary<string, string> LdapGroupToAppRole { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> AppRolePriority { get; set; } = new() { "Admin", "Manager", "Receptionist", "Rengøring", "Kunde", "InfiniteVoid" };
    }
}
