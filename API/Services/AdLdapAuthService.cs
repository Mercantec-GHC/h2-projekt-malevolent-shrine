using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using API.AD;
using API.Data;
using API.Models;
using System.Linq;

namespace API.Services
{
    public class AdLdapAuthService
    {
        private readonly IConfiguration _config;
        private readonly ActiveDirectoryService _adService;
        private readonly AppDBContext _db;
        private readonly JwtService _jwt;
        private readonly ILogger<AdLdapAuthService> _logger;

        public AdLdapAuthService(
            IConfiguration config,
            ActiveDirectoryService adService,
            AppDBContext db,
            JwtService jwt,
            ILogger<AdLdapAuthService> logger)
        {
            _config = config;
            _adService = adService;
            _db = db;
            _jwt = jwt;
            _logger = logger;
        }

        public async Task<DTOs.AdLoginResponseDto> LoginWithAdAsync(string username, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            TryBindWithUser(username, password);
            var adUser = FindAdUserBySamAccountName(username);
            var adGroups = _adService.GetUserGroups(username) ?? new List<string>();
            var mapping = ReadRoleMapping();
            var mappedRoles = adGroups
                .Select(g => mapping.LdapGroupToAppRole.TryGetValue(g, out var appRole) ? appRole : null)
                .Where(r => !string.IsNullOrEmpty(r))
                .Cast<string>()
                .Distinct()
                .ToList();
            var primaryRole = ChoosePrimaryRole(mappedRoles, mapping);
            var user = await EnsureLocalUserAsync(adUser, primaryRole, ct);
            var accessToken = _jwt.GenerateToken(user, roleName: user.Role?.Name ?? primaryRole ?? RoleNames.Kunde);
            return new DTOs.AdLoginResponseDto
            {
                Message = "Успешный вход через Active Directory",
                AccessToken = accessToken,
                AccessTokenExpiry = _jwt.GetAccessTokenExpiry(),
                Email = adUser.Email,
                DisplayName = string.IsNullOrEmpty(adUser.DisplayName) ? adUser.Name : adUser.DisplayName,
                Username = adUser.Username,
                AdGroups = adGroups,
                AppRole = user.Role?.Name ?? primaryRole
            };
        }

        private void TryBindWithUser(string username, string password)
        {
            var domain = _config["ActiveDirectory:Domain"] ?? _adService.Config.Domain;
            var server = _config["ActiveDirectory:Server"] ?? _adService.Config.Server;
            var credential = new NetworkCredential($"{username}@{domain}", password);
            using var connection = new LdapConnection(server)
            {
                Credential = credential,
                AuthType = AuthType.Negotiate,
                SessionOptions = { ProtocolVersion = 3 }
            };
            try
            {
                connection.Bind();
            }
            catch (LdapException lex)
            {
                _logger.LogWarning(lex, "LDAP bind failed for user {User}", username);
                throw new UnauthorizedAccessException("Неверные учетные данные Active Directory.");
            }
        }

        private AD.ADUser FindAdUserBySamAccountName(string username)
        {
            var results = _adService.SearchUsers(username) ?? new List<AD.ADUser>();
            var exact = results.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            return exact ?? results.FirstOrDefault() ?? new AD.ADUser { Username = username };
        }

        private AdRoleMappingOptions ReadRoleMapping()
        {
            var opts = new AdRoleMappingOptions();
            var section = _config.GetSection("ADRoleMapping:LdapGroupToAppRole");
            if (section.Exists())
            {
                foreach (var child in section.GetChildren())
                {
                    if (!string.IsNullOrEmpty(child.Key) && !string.IsNullOrEmpty(child.Value))
                        opts.LdapGroupToAppRole[child.Key] = child.Value!;
                }
            }
            else
            {
                opts.LdapGroupToAppRole["HotelAdmins"] = RoleNames.Admin;
                opts.LdapGroupToAppRole["Reception"] = RoleNames.Manager;
                opts.LdapGroupToAppRole["Staff"] = RoleNames.Receptionist;
            }
            var prioSection = _config.GetSection("ADRoleMapping:AppRolePriority");
            if (prioSection.Exists())
            {
                opts.AppRolePriority = prioSection.Get<List<string>>() ?? opts.AppRolePriority;
            }
            return opts;
        }

        private static string ChoosePrimaryRole(List<string> roles, AdRoleMappingOptions mapping)
        {
            if (roles.Count == 0) return RoleNames.Kunde;
            foreach (var pref in mapping.AppRolePriority)
            {
                var match = roles.FirstOrDefault(r => string.Equals(r, pref, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }
            return roles.First();
        }

        private async Task<User> EnsureLocalUserAsync(AD.ADUser adUser, string primaryRole, CancellationToken ct)
        {
            var lowerEmail = string.IsNullOrWhiteSpace(adUser.Email) ? null : adUser.Email.ToLowerInvariant();
            User? user = null;
            if (!string.IsNullOrEmpty(lowerEmail))
            {
                user = await _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == lowerEmail, ct);
            }
            if (user == null)
            {
                user = await _db.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == adUser.Username, ct);
            }
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == primaryRole, ct);
            if (role == null)
            {
                role = new Role { Name = primaryRole };
                _db.Roles.Add(role);
                await _db.SaveChangesAsync(ct);
            }
            if (user == null)
            {
                user = new User
                {
                    Username = adUser.Username,
                    Email = lowerEmail ?? $"{adUser.Username}@{_adService.Config.Domain}",
                    FirstName = adUser.FirstName,
                    LastName = adUser.LastName,
                    RoleId = role.Id,
                };
                _db.Users.Add(user);
            }
            else
            {
                if (user.RoleId != role.Id)
                {
                    user.RoleId = role.Id;
                    user.UpdatedAt = DateTime.UtcNow;
                }
                if (!string.IsNullOrEmpty(adUser.FirstName)) user.FirstName = adUser.FirstName;
                if (!string.IsNullOrEmpty(adUser.LastName)) user.LastName = adUser.LastName;
                if (!string.IsNullOrEmpty(lowerEmail)) user.Email = lowerEmail;
            }
            await _db.SaveChangesAsync(ct);
            user = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == user.Id, ct);
            return user;
        }
    }
}
