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
            List<string> adGroups;
            try
            {
                adGroups = _adService.GetUserGroups(username) ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch AD groups for user {User}. Proceeding with empty roles.", username);
                adGroups = new List<string>();
            }
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
            if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(domain))
            {
                _logger.LogError("LDAP config is missing. Server='{Server}', Domain='{Domain}'", server, domain);
                throw new InvalidOperationException("Конфигурация Active Directory не задана (server/domain). Обратитесь к администратору.");
            }

            _logger.LogInformation("Attempting LDAP bind. Server='{Server}', Domain='{Domain}', Username='{User}'", server, domain, username);

            // Пытаемся последовательно UPN и DOMAIN+username
            var tries = new List<Func<LdapConnection>>
            {
                () => BuildLdapConnection(server, new NetworkCredential($"{username}@{domain}", password)),
                () => BuildLdapConnection(server, new NetworkCredential(username, password, domain))
            };

            List<Exception> errors = new();
            foreach (var attempt in tries)
            {
                try
                {
                    using var conn = attempt();
                    conn.Bind();
                    _logger.LogInformation("LDAP bind succeeded for user '{User}'", username);
                    return;
                }
                catch (LdapException lex)
                {
                    errors.Add(lex);
                    var sub = ParseAdSubError(lex.ServerErrorMessage);
                    _logger.LogWarning(lex, "LDAP bind failed ({Code}) for user {User} on {Server}. SubError={Sub}", lex.ErrorCode, username, server, sub?.Code);
                    // Если явная проблема с сетью/сервером — нет смысла пробовать дальше
                    if (lex.ErrorCode == 81 /* LDAP_SERVER_DOWN */)
                        break;
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                    _logger.LogWarning(ex, "LDAP bind unexpected error for user {User} on {Server}", username, server);
                }
            }

            // Формируем человекочитаемое сообщение
            var (clientMsg, isAuthIssue) = BestAuthMessageFrom(errors);
            if (isAuthIssue)
                throw new UnauthorizedAccessException(clientMsg);

            throw new Exception(clientMsg);
        }

        private LdapConnection BuildLdapConnection(string server, NetworkCredential cred)
        {
            var connection = new LdapConnection(server)
            {
                Credential = cred,
                AuthType = AuthType.Negotiate
            };
            connection.SessionOptions.ProtocolVersion = 3;
            return connection;
        }

        private record AdSubError(string Code, string? Raw);
        private AdSubError? ParseAdSubError(string? serverErrorMessage)
        {
            if (string.IsNullOrEmpty(serverErrorMessage)) return null;
            // Ищем под‑код вида data 52e, data 775 и т.п.
            // Примеры: "000004DC: LdapErr: DSID-031A1253, comment: In order to perform this operation a successful bind must be completed on the connection., data 0, v2580"
            //          "80090308: LdapErr: DSID-031A1235, data 52e, v2580"
            var msg = serverErrorMessage.ToLowerInvariant();
            var idx = msg.IndexOf("data ");
            if (idx >= 0 && idx + 5 < msg.Length)
            {
                var start = idx + 5;
                var end = start;
                while (end < msg.Length && (char.IsLetterOrDigit(msg[end]) || msg[end] == 'x')) end++;
                var code = msg.Substring(start, end - start).Trim();
                return new AdSubError(code, serverErrorMessage);
            }
            return new AdSubError("", serverErrorMessage);
        }

        private (string message, bool isUnauthorized) BestAuthMessageFrom(IEnumerable<Exception> errors)
        {
            // Предпочитаем LDAP ошибки
            foreach (var err in errors)
            {
                if (err is LdapException lex)
                {
                    var sub = ParseAdSubError(lex.ServerErrorMessage);
                    // Мэппинг распространенных под‑кодов AD при ErrorCode=49 (InvalidCredentials)
                    if (lex.ErrorCode == 49)
                    {
                        var code = sub?.Code;
                        return code switch
                        {
                            "525" => ("Пользователь не найден в Active Directory.", true),
                            "52e" => ("Неверные учетные данные Active Directory.", true),
                            "530" => ("Вход не разрешен в текущее время.", true),
                            "531" => ("Вход не разрешен на этой рабочей станции.", true),
                            "532" => ("Срок действия пароля истек. Обновите пароль.", true),
                            "533" => ("Учетная запись отключена. Обратитесь к администратору.", true),
                            "701" => ("Срок действия учетной записи истек.", true),
                            "773" => ("Требуется смена пароля при входе.", true),
                            "775" => ("Учетная запись заблокирована. Обратитесь к администратору.", true),
                            _ => ("Неверные учетные данные Active Directory.", true)
                        };
                    }

                    if (lex.ErrorCode == 81)
                    {
                        return ($"Не удается подключиться к LDAP-серверу '{_adService.Config.Server}'. Проверьте адрес/порт/сетевую доступность.", false);
                    }

                    // Прочие LDAP ошибки
                    return ($"Ошибка LDAP: {lex.Message}", lex.ErrorCode == 49);
                }
            }

            var last = errors.LastOrDefault();
            return (last?.Message ?? "Неизвестная ошибка при подключении к AD.", false);
        }

        private AD.ADUser FindAdUserBySamAccountName(string username)
        {
            try
            {
                var results = _adService.SearchUsers(username) ?? new List<AD.ADUser>();
                var exact = results.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
                return exact ?? results.FirstOrDefault() ?? new AD.ADUser { Username = username };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AD search failed for user {User}. Falling back to minimal profile.", username);
                return new AD.ADUser { Username = username };
            }
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
                    HashedPassword = "AD" // <--- фиксация поля для AD-пользователей
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
