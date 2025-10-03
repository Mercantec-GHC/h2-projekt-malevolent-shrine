// Standalone additions for AD (LDAP) login + JWT + role mapping + admin usage
// Placeholders are wired to existing API and Blazor projects. Move parts into proper projects/files as needed.
// This file is at repo root to avoid impacting current builds. Copy pieces into API and Blazor projects explicitly.

// ============================= API side (server) =============================

#if SERVER_SNIPPETS
using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using API.AD;              // Reuse existing ActiveDirectoryService for groups/search
using API.Data;            // AppDBContext
using API.Models;          // User, Role
using API.Services;        // JwtService
#endif

namespace API.Additions
{
    // Input from client for AD login
    public class AdLoginRequestDto
    {
        public string Username { get; set; } = string.Empty; // samAccountName, e.g. jdoe
        public string Password { get; set; } = string.Empty; // user's AD password
    }

    // Response back to client
    public class AdLoginResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiry { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Username { get; set; }
        public List<string> AdGroups { get; set; } = new();
        public string? AppRole { get; set; } // primary mapped role used for JWT claim
    }

    // Mapping config between AD groups and application roles
    // Move this into appsettings.json (section "ADRoleMapping") when wiring.
    public class AdRoleMappingOptions
    {
        // Example: { "HotelAdmins": "Admin", "Reception": "Manager" }
        public Dictionary<string, string> LdapGroupToAppRole { get; set; } = new();

        // Priority of roles when a user matches multiple AD groups; first match is primary
        public List<string> AppRolePriority { get; set; } = new() { "Admin", "Manager", "Staff", "User" };
    }

#if SERVER_SNIPPETS
    // Service encapsulating AD credential validation, group retrieval, mapping to app roles, and local user sync
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

        // Contract:
        // - Validates AD credentials via LDAP bind using UPN (username@domain)
        // - If success: fetches AD groups, maps to app role, ensures local user exists, returns JWT and profile
        public async Task<AdLoginResponseDto> LoginWithAdAsync(string username, string password, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            // 1) Validate credentials against AD with user-provided password
            TryBindWithUser(username, password);

            // 2) Load AD user details and groups
            var adUser = FindAdUserBySamAccountName(username);
            var adGroups = _adService.GetUserGroups(username) ?? new List<string>();

            // 3) Map AD groups -> application role
            var mapping = ReadRoleMapping();
            var mappedRoles = adGroups
                .Select(g => mapping.LdapGroupToAppRole.TryGetValue(g, out var appRole) ? appRole : null)
                .Where(r => !string.IsNullOrEmpty(r))
                .Cast<string>()
                .Distinct()
                .ToList();

            var primaryRole = ChoosePrimaryRole(mappedRoles, mapping);

            // 4) Ensure local user exists and has a role
            var user = await EnsureLocalUserAsync(adUser, primaryRole, ct);

            // 5) Issue JWT (uses single role name per current JwtService API)
            var accessToken = _jwt.GenerateToken(user, roleName: user.Role?.Name ?? primaryRole ?? RoleNames.User);

            return new AdLoginResponseDto
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

        // Attempt LDAP bind with the end-user credentials
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

        // Find user by samAccountName using existing AD service
        private AD.ADUser FindAdUserBySamAccountName(string username)
        {
            var results = _adService.SearchUsers(username) ?? new List<AD.ADUser>();
            // Prefer exact samAccountName match if possible
            var exact = results.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
            return exact ?? results.FirstOrDefault() ?? new AD.ADUser { Username = username };
        }

        private AdRoleMappingOptions ReadRoleMapping()
        {
            var opts = new AdRoleMappingOptions();

            // Read mapping from config section ADRoleMapping:LdapGroupToAppRole
            var section = _config.GetSection("ADRoleMapping:LdapGroupToAppRole");
            if (section.Exists())
            {
                foreach (var child in section.GetChildren())
                {
                    // child.Key = AD group name, child.Value = app role
                    if (!string.IsNullOrEmpty(child.Key) && !string.IsNullOrEmpty(child.Value))
                        opts.LdapGroupToAppRole[child.Key] = child.Value!;
                }
            }
            else
            {
                // Defaults (adjust as needed)
                opts.LdapGroupToAppRole["HotelAdmins"] = RoleNames.Admin;
                opts.LdapGroupToAppRole["Reception"] = RoleNames.Manager;
                opts.LdapGroupToAppRole["Staff"] = RoleNames.Staff;
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
            if (roles.Count == 0) return RoleNames.User;
            foreach (var pref in mapping.AppRolePriority)
            {
                var match = roles.FirstOrDefault(r => string.Equals(r, pref, StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }
            return roles.First();
        }

        private async Task<User> EnsureLocalUserAsync(AD.ADUser adUser, string primaryRole, CancellationToken ct)
        {
            // Try by email first, then by username
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

            // Ensure role exists
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
                // Update role if changed
                if (user.RoleId != role.Id)
                {
                    user.RoleId = role.Id;
                    user.UpdatedAt = DateTime.UtcNow;
                }

                // Sync optional profile fields
                if (!string.IsNullOrEmpty(adUser.FirstName)) user.FirstName = adUser.FirstName;
                if (!string.IsNullOrEmpty(adUser.LastName)) user.LastName = adUser.LastName;
                if (!string.IsNullOrEmpty(lowerEmail)) user.Email = lowerEmail;
            }

            await _db.SaveChangesAsync(ct);

            // Reload role navigation
            user = await _db.Users.Include(u => u.Role).FirstAsync(u => u.Id == user.Id, ct);
            return user;
        }
    }

    // Controller dedicated for AD login endpoint (kept separate to avoid modifying existing AuthController)
    [ApiController]
    [Route("api/adauth")] // POST api/adauth/login
    public class AdAuthController : ControllerBase
    {
        private readonly AdLdapAuthService _service;
        public AdAuthController(AdLdapAuthService service)
        {
            _service = service;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AdLoginResponseDto>> Login([FromBody] AdLoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                var res = await _service.LoginWithAdAsync(dto.Username, dto.Password, ct);
                return Ok(res);
            }
            catch (UnauthorizedAccessException uae)
            {
                return Unauthorized(new { error = uae.Message });
            }
            catch (ArgumentException aex)
            {
                return BadRequest(new { error = aex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
#endif
}

// Recommended wiring (move into Program.cs / DI on the API side):
// services.AddScoped<API.Additions.AdLdapAuthService>();
// Optionally bind role mapping from config: section "ADRoleMapping"
// Example appsettings.json snippet:
/*
  "ADRoleMapping": {
    "LdapGroupToAppRole": {
      "HotelAdmins": "Admin",
      "Reception": "Manager",
      "Staff": "Staff"
    },
    "AppRolePriority": [ "Admin", "Manager", "Staff", "User" ]
  }
*/

// Recommended role protection for AD endpoints (apply manually):
// [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)] on ActiveDirectoryController to limit access.


// ============================= Blazor side (client) =============================

#if BLAZOR_SNIPPETS
using System.Net.Http.Json;
using Microsoft.JSInterop;
namespace Blazor.Services
{
    // Client helper to call AD login endpoint and store JWT in localStorage
    public class AdAuthClientService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private const string TOKEN_KEY = "access_token"; // matches existing usage

        public AdAuthClientService(HttpClient http, IJSRuntime js)
        {
            _http = http; _js = js;
        }

        public async Task<bool> LoginWithAdAsync(string username, string password)
        {
            var payload = new API.Additions.AdLoginRequestDto { Username = username, Password = password };
            var response = await _http.PostAsJsonAsync("api/adauth/login", payload);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<API.Additions.AdLoginResponseDto>();
            if (result == null || string.IsNullOrEmpty(result.AccessToken)) return false;

            await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, result.AccessToken);
            return true;
        }
    }
}
#endif

// Notes per lecture and ITIL reminders (comments only):
// - AD connectivity test exists already in API (ActiveDirectoryController.TestConnection), dashboard button calls it.
// - This file adds the missing AD login + role mapping flow, keeping code separate.
// - ITIL: focus on user value (receptionist admin view), start from existing controllers, iterate and test.
// - Docker: LDAP might not work in hosted envs due to network constraints; keep AD login for local demo if needed.

// =============================
// INTEGRATION GUIDE (READ ME)
// =============================
// Purpose: This file contains all missing pieces for AD login via LDAP + JWT + role mapping.
// You will COPY specific classes/blocks from here into your solution. Do not include #if ... #endif wrappers.
// No code in your projects is changed by this file until you copy the pieces as instructed below.
//
// IMPORTANT: In your repo we already found these files:
// - API/DTOs/AdLoginDto.cs (AdLoginRequestDto, AdLoginResponseDto) — already created. Do NOT duplicate them.
// - API/Controllers/ActiveDirectoryController.cs — currently [Authorize] without role restriction.
// - API/Services/JwtService.cs — already issues JWT with a single role claim.
// - API/AD/* — full LDAP access (users, groups, search, connection).
// - Blazor/Pages/AdminDashboard.razor — calls API AD endpoints using JWT from localStorage.
//
// WHAT YOU WILL ADD (copy from this file):
// 1) AdRoleMappingOptions class  → place to: API/Services/AdRoleMappingOptions.cs (or API/Models)
// 2) AdLdapAuthService class     → place to: API/Services/AdLdapAuthService.cs
// 3) AdAuthController class      → place to: API/Controllers/AdAuthController.cs
// 4) AdAuthClientService class   → place to: Blazor/Services/AdAuthClientService.cs (optional frontend helper)
// 5) Program.cs wiring           → DI registration for AdLdapAuthService
// 6) appsettings.json            → add ADRoleMapping section
// 7) Access control (optional)   → tighten [Authorize(Roles=...)] on AD endpoints
//
// CONTRACT (what the new flow does):
// - Input: username (samAccountName) + password (AD user creds) sent to POST api/adauth/login.
// - Auth: validates by LDAP bind as username@domain; on success fetches AD groups.
// - Mapping: maps AD groups → app roles via ADRoleMapping config (first priority match becomes primary role).
// - Local user: finds/creates/updates local User in DB and ensures Role exists.
// - Output: issues JWT (existing JwtService) with role claim; returns token and profile info to client.
//
// EDGE CASES handled in service:
// - Invalid AD creds → 401; missing username/password → 400; no mapped roles → defaults to RoleNames.User;
// - Missing email in AD → fallback email username@<domain> to satisfy non-null DB constraints; idempotent user sync.
//
// STEP-BY-STEP (copy instructions)
// -----------------------------------------------------------------------------
// Step 0 — SKIP creating DTOs
// You already have: API/DTOs/AdLoginDto.cs with AdLoginRequestDto and AdLoginResponseDto.
// If you ever need their shapes, see the equivalents inside this file under namespace API.Additions.
// -----------------------------------------------------------------------------
// Step 1 — Add group→role mapping options
// Copy ONLY this class (without any #if / #endif wrappers) into a new file:
//   API/Services/AdRoleMappingOptions.cs
// From below:
//   namespace API.Additions { public class AdRoleMappingOptions { ... } }
// You may change its namespace to API.Services or API.Models if you prefer; update usings accordingly.
// -----------------------------------------------------------------------------
// Step 2 — Add LDAP auth service (server)
// Copy ONLY class AdLdapAuthService (and required usings) into new file:
//   API/Services/AdLdapAuthService.cs
// Keep its namespace consistent (e.g., namespace API.Additions or API.Services). Ensure these dependencies are available:
//   - IConfiguration, ILogger<AdLdapAuthService>
//   - ActiveDirectoryService (already registered in Program.cs)
//   - AppDBContext (registered)
//   - JwtService (registered)
// Notes:
//   - It reuses your API.AD.ActiveDirectoryService for GetUserGroups and searching the user.
//   - It binds with end-user creds (username@domain) to validate login, not the admin bind.
//   - It reads ADRoleMapping from config; defaults are provided if the section is absent.
// -----------------------------------------------------------------------------
// Step 3 — Add controller for AD login (server)
// Copy ONLY class AdAuthController (and its using directives) into a new file:
//   API/Controllers/AdAuthController.cs
// Route will be: POST api/adauth/login — it returns AdLoginResponseDto with the AccessToken.
// Do NOT replace your existing AuthController; they will coexist (DB login vs AD login).
// -----------------------------------------------------------------------------
// Step 4 — Register service in DI (server)
// Edit file: API/Program.cs
// Find where services are registered (builder.Services...). Add this line near JwtService registration:
//   builder.Services.AddScoped<API.Additions.AdLdapAuthService>();
// If you moved the namespace to API.Services, use that instead:
//   builder.Services.AddScoped<AdLdapAuthService>();
// No other changes are required to run the endpoint.
// -----------------------------------------------------------------------------
// Step 5 — Add config for role mapping (server)
// Edit file: API/appsettings.json and add a sibling section at root level (next to "Jwt", "ActiveDirectory"):
//   "ADRoleMapping": {
//     "LdapGroupToAppRole": {
//       "HotelAdmins": "Admin",
//       "Reception": "Manager",
//       "Staff": "Staff"
//     },
//     "AppRolePriority": [ "Admin", "Manager", "Staff", "User" ]
//   }
// You can later rename keys to your actual AD group names.
// -----------------------------------------------------------------------------
// Step 6 — Restrict AD endpoints to roles (optional but recommended)
// Edit: API/Controllers/ActiveDirectoryController.cs
// At class level, replace existing [Authorize] with:
//   [Authorize(Roles = RoleNames.Admin + "," + RoleNames.Manager)]
// This keeps read-access to AD data limited to staff.
// -----------------------------------------------------------------------------
// Step 7 — Frontend (Blazor) helper for AD login (optional)
// Create file: Blazor/Services/AdAuthClientService.cs
// Copy ONLY class AdAuthClientService from below (inside #if BLAZOR_SNIPPETS) into it, remove the #if/#endif.
// Then register HttpClient for it in Blazor/Program.cs (alongside APIService and ActiveDirectoryService):
//   builder.Services.AddHttpClient<AdAuthClientService>(client =>
//   {
//       client.BaseAddress = new Uri(apiEndpoint);
//   });
// Usage in Login.razor (example): inject AdAuthClientService and call LoginWithAdAsync(username, password),
// which stores the returned JWT into localStorage under key "access_token".
// -----------------------------------------------------------------------------
// Step 8 — Secrets hygiene (strongly recommended)
// In API/appsettings.json you currently have ActiveDirectory:Username/Password and Jwt:SecretKey in plain text.
// Move them to environment variables or Secret Manager:
//   ActiveDirectory__Server, ActiveDirectory__Domain, ActiveDirectory__Username, ActiveDirectory__Password
//   JWT__SecretKey, JWT__Issuer, JWT__Audience, JWT__ExpiryMinutes, JWT__RefreshTokenExpiryDays
// Your Program.cs already reads Jwt values from env vars when present.
// -----------------------------------------------------------------------------
// Smoke test checklist
// - POST api/auth/login (DB login) still works as before — unchanged.
// - POST api/adauth/login with valid AD user returns 200 and AccessToken.
// - Call GET api/activedirectory/users with Authorization: Bearer <token> to verify access.
// - Optional: after Step 6, try with a non-staff token and expect 403.
// - Blazor Admin Dashboard loads users/groups after token is present in localStorage.
// -----------------------------------------------------------------------------
// Troubleshooting
// - 401 on api/adauth/login: wrong AD creds or LDAP bind blocked by network.
// - 500 on login: ensure DI registered AdLdapAuthService; ensure ADRoleMapping JSON is valid.
// - No roles mapped: adjust ADRoleMapping.LdapGroupToAppRole keys to match exact AD group CNs.
// - Docker/hosted: LDAP may fail due to network/firewall; instructor notes say localhost works; test locally.
// ============================= END GUIDE =============================
