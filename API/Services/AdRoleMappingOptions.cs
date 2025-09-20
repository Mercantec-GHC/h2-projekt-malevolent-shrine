namespace API.Services
{
    public class AdRoleMappingOptions
    {
        public Dictionary<string, string> LdapGroupToAppRole { get; set; } = new();
        public List<string> AppRolePriority { get; set; } = new() { "Admin", "Manager", "Receptionist", "Rengøring", "Kunde", "InfiniteVoid" };
    }
}
