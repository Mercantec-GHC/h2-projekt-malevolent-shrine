using System.DirectoryServices.Protocols;

namespace API.AD
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Henter alle brugere fra Active Directory
        /// </summary>
        /// <returns>Liste af ADUser objekter</returns>
        public List<ADUser> GetAllUsers()
        {
            var users = new List<ADUser>();

            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    "(objectClass=user)",
                    SearchScope.Subtree,
                    "cn",
                    "samAccountName",
                    "mail",
                    "department",
                    "title",
                    "distinguishedName"
                );

                try
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        var user = new ADUser
                        {
                            Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                            Username = entry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                            Email = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                            Department = entry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                            Title = entry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                            DistinguishedName = entry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A"
                        };

                        users.Add(user);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af brugere: {ex.Message}");
                }
            }

            return users;
        }

        /// <summary>
        /// Søger efter brugere baseret på søgeterm
        /// </summary>
        /// <param name="searchTerm">Søgeterm til at søge efter</param>
        /// <returns>Liste af matchende ADUser objekter</returns>
        public List<ADUser> SearchUsers(string searchTerm)
        {
            var users = new List<ADUser>();

            using (var connection = GetConnection())
            {
                var filter = $"(|(cn=*{searchTerm}*)(mail=*{searchTerm}*)(department=*{searchTerm}*)(title=*{searchTerm}*))";
                
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    filter,
                    SearchScope.Subtree,
                    "cn",
                    "samAccountName",
                    "mail",
                    "department",
                    "title",
                    "distinguishedName",
                    "givenName",
                    "sn",
                    "displayName",
                    "company",
                    "physicalDeliveryOfficeName",
                    "telephoneNumber",
                    "mobile",
                    "manager",
                    "lastLogon",
                    "pwdLastSet",
                    "userAccountControl"
                );

                try
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        var user = new ADUser
                        {
                            Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                            Username = entry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                            Email = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                            Department = entry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                            Title = entry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                            DistinguishedName = entry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A",
                            FirstName = entry.Attributes["givenName"]?[0]?.ToString() ?? "N/A",
                            LastName = entry.Attributes["sn"]?[0]?.ToString() ?? "N/A",
                            DisplayName = entry.Attributes["displayName"]?[0]?.ToString() ?? "N/A",
                            Company = entry.Attributes["company"]?[0]?.ToString() ?? "N/A",
                            Office = entry.Attributes["physicalDeliveryOfficeName"]?[0]?.ToString() ?? "N/A",
                            Phone = entry.Attributes["telephoneNumber"]?[0]?.ToString() ?? "N/A",
                            Mobile = entry.Attributes["mobile"]?[0]?.ToString() ?? "N/A",
                            Manager = entry.Attributes["manager"]?[0]?.ToString() ?? "N/A"
                        };

                        // Parse lastLogon (kan være byte array)
                        if (entry.Attributes.Contains("lastLogon"))
                        {
                            var lastLogonValue = entry.Attributes["lastLogon"][0];
                            if (lastLogonValue is byte[] lastLogonBytes && lastLogonBytes.Length == 8)
                            {
                                var ticks = BitConverter.ToInt64(lastLogonBytes, 0);
                                if (ticks > 0)
                                {
                                    user.LastLogon = DateTime.FromFileTime(ticks);
                                }
                            }
                        }

                        // Parse passwordLastSet
                        if (entry.Attributes.Contains("pwdLastSet"))
                        {
                            var pwdLastSetValue = entry.Attributes["pwdLastSet"][0];
                            if (pwdLastSetValue is byte[] pwdLastSetBytes && pwdLastSetBytes.Length == 8)
                            {
                                var ticks = BitConverter.ToInt64(pwdLastSetBytes, 0);
                                if (ticks > 0)
                                {
                                    user.PasswordLastSet = DateTime.FromFileTime(ticks);
                                }
                            }
                        }

                        // Parse userAccountControl for enabled status
                        if (entry.Attributes.Contains("userAccountControl"))
                        {
                            var uacValue = entry.Attributes["userAccountControl"][0]?.ToString();
                            if (int.TryParse(uacValue, out int uac))
                            {
                                // Bit 2 (0x0002) = ACCOUNTDISABLE
                                user.IsEnabled = (uac & 0x0002) == 0;
                            }
                        }

                        users.Add(user);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved søgning efter brugere: {ex.Message}");
                }
            }

            return users;
        }

        /// <summary>
        /// Viser alle brugere i konsollen
        /// </summary>
        public void ShowAllUsers()
        {
            Console.WriteLine("=== Alle Brugere ===");
            var users = GetAllUsers();
            
            if (users.Count == 0)
            {
                Console.WriteLine("Ingen brugere fundet.");
                return;
            }

            Console.WriteLine($"Fundet {users.Count} brugere:");
            Console.WriteLine();
            
            foreach (var user in users.OrderBy(u => u.Name))
            {
                Console.WriteLine($"Navn: {user.Name}");
                Console.WriteLine($"Brugernavn: {user.Username}");
                Console.WriteLine($"Email: {user.Email}");
                Console.WriteLine($"Afdeling: {user.Department}");
                Console.WriteLine($"Titel: {user.Title}");
                Console.WriteLine(new string('-', 50));
            }
        }

        /// <summary>
        /// Søger og viser brugere baseret på søgeterm
        /// </summary>
        public void SearchUsers()
        {
            Console.WriteLine("=== Søg efter Brugere ===");
            Console.Write("Indtast søgekriterie (navn, email, afdeling, eller titel): ");
            var searchTerm = Console.ReadLine();

            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine("Søgeterm kan ikke være tom.");
                return;
            }

            var users = SearchUsers(searchTerm);
            
            if (users.Count == 0)
            {
                Console.WriteLine("Ingen brugere fundet med det søgekriterie.");
                return;
            }

            Console.WriteLine($"Fundet {users.Count} brugere:");
            Console.WriteLine();
            
            foreach (var user in users.OrderBy(u => u.Name))
            {
                ShowUserDetails(user);
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Viser detaljerede oplysninger om en bruger (samme format som /me)
        /// </summary>
        /// <param name="user">Brugeren at vise oplysninger for</param>
        public void ShowUserDetails(ADUser user)
        {
            try
            {
                // Hent brugerens grupper
                Console.WriteLine("Henter brugerens roller/grupper...");
                var groups = GetUserGroups(user.Username);
                Console.WriteLine($"Fundet {groups.Count} roller/grupper");

                // Grundlæggende information
                Console.WriteLine("👤 GRUNDLÆGGENDE INFORMATION");
                Console.WriteLine($"   Navn: {user.DisplayName}");
                Console.WriteLine($"   Brugernavn: {user.Username}");
                Console.WriteLine($"   Email: {user.Email}");
                Console.WriteLine($"   Firma: {user.Company}");
                Console.WriteLine($"   Afdeling: {user.Department}");
                Console.WriteLine($"   Titel: {user.Title}");
                Console.WriteLine($"   Kontor: {user.Office}");
                Console.WriteLine();

                // Kontakt information
                Console.WriteLine("📞 KONTAKT INFORMATION");
                Console.WriteLine($"   Telefon: {user.Phone}");
                Console.WriteLine($"   Mobil: {user.Mobile}");
                Console.WriteLine($"   Manager: {user.Manager}");
                Console.WriteLine();

                // Konto status
                Console.WriteLine("🔐 KONTO STATUS");
                Console.WriteLine($"   Status: {(user.IsEnabled ? "✅ Aktiv" : "❌ Deaktiveret")}");
                if (user.LastLogon.HasValue)
                {
                    Console.WriteLine($"   Sidste login: {user.LastLogon.Value:dd/MM/yyyy HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine("   Sidste login: Aldrig");
                }
                if (user.PasswordLastSet.HasValue)
                {
                    Console.WriteLine($"   Adgangskode ændret: {user.PasswordLastSet.Value:dd/MM/yyyy HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine("   Adgangskode ændret: Ukendt");
                }
                Console.WriteLine();

                // Grupper/Roller
                Console.WriteLine($"👥 ROLLER/GRUPPER ({groups.Count})");
                if (groups.Count > 0)
                {
                    foreach (var group in groups.OrderBy(g => g))
                    {
                        Console.WriteLine($"   • {group}");
                    }
                }
                else
                {
                    Console.WriteLine("   Ingen roller/grupper fundet");
                }
                Console.WriteLine();

                // Tekniske detaljer
                Console.WriteLine("🔧 TEKNISKE DETALJER");
                Console.WriteLine($"   Distinguished Name: {user.DistinguishedName}");
                Console.WriteLine($"   Forbindelse: {_config.Server}");
                Console.WriteLine($"   Domæne: {_config.Domain}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fejl ved hentning af brugeroplysninger: {ex.Message}");
            }
        }

        /// <summary>
        /// Henter grupper for en specifik bruger
        /// </summary>
        /// <param name="username">Brugernavnet</param>
        /// <returns>Liste af gruppenavne</returns>
        public List<string> GetUserGroups(string username)
        {
            var groups = new List<string>();

            using (var connection = GetConnection())
            {
                // Først find brugerens distinguished name
                var userSearchRequest = new SearchRequest(
                    GetBaseDN(),
                    $"(samAccountName={username})",
                    SearchScope.Subtree,
                    "distinguishedName"
                );

                try
                {
                    var userResponse = (SearchResponse)connection.SendRequest(userSearchRequest);
                    
                    if (userResponse.Entries.Count == 0)
                    {
                        return groups;
                    }

                    var userDN = userResponse.Entries[0].Attributes["distinguishedName"][0]?.ToString();
                    if (string.IsNullOrEmpty(userDN))
                    {
                        return groups;
                    }

                    // Søg efter grupper hvor brugeren er medlem
                    var groupSearchRequest = new SearchRequest(
                        GetBaseDN(),
                        $"(member={userDN})",
                        SearchScope.Subtree,
                        "cn",
                        "description"
                    );

                    var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

                    foreach (SearchResultEntry entry in groupResponse.Entries)
                    {
                        if (entry.Attributes.Contains("cn"))
                        {
                            var groupName = entry.Attributes["cn"][0]?.ToString();
                            if (!string.IsNullOrEmpty(groupName))
                            {
                                groups.Add(groupName);
                            }
                        }
                    }

                    // Prøv også med recursive group membership (hvis understøttet)
                    try
                    {
                        var recursiveGroupSearchRequest = new SearchRequest(
                            GetBaseDN(),
                            $"(member:1.2.840.113556.1.4.1941:={userDN})",
                            SearchScope.Subtree,
                            "cn"
                        );

                        var recursiveResponse = (SearchResponse)connection.SendRequest(recursiveGroupSearchRequest);

                        foreach (SearchResultEntry entry in recursiveResponse.Entries)
                        {
                            if (entry.Attributes.Contains("cn"))
                            {
                                var groupName = entry.Attributes["cn"][0]?.ToString();
                                if (!string.IsNullOrEmpty(groupName) && !groups.Contains(groupName))
                                {
                                    groups.Add(groupName);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignorer fejl for recursive search - det er ikke altid understøttet
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fejl ved hentning af grupper for {username}: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Henter detaljerede oplysninger om den aktuelle bruger
        /// </summary>
        /// <returns>ADUser objekt med detaljerede oplysninger</returns>
        public ADUser GetCurrentUserDetails()
        {
            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    $"(samAccountName={_config.Username})",
                    SearchScope.Subtree,
                    "cn",
                    "samAccountName",
                    "mail",
                    "department",
                    "title",
                    "distinguishedName",
                    "givenName",
                    "sn",
                    "displayName",
                    "company",
                    "physicalDeliveryOfficeName",
                    "telephoneNumber",
                    "mobile",
                    "manager",
                    "lastLogon",
                    "pwdLastSet",
                    "userAccountControl"
                );

                try
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    if (response.Entries.Count == 0)
                    {
                        throw new Exception("Bruger ikke fundet i Active Directory");
                    }

                    var entry = response.Entries[0];
                    var user = new ADUser
                    {
                        Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                        Username = entry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                        Email = entry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                        Department = entry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                        Title = entry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                        DistinguishedName = entry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A",
                        FirstName = entry.Attributes["givenName"]?[0]?.ToString() ?? "N/A",
                        LastName = entry.Attributes["sn"]?[0]?.ToString() ?? "N/A",
                        DisplayName = entry.Attributes["displayName"]?[0]?.ToString() ?? "N/A",
                        Company = entry.Attributes["company"]?[0]?.ToString() ?? "N/A",
                        Office = entry.Attributes["physicalDeliveryOfficeName"]?[0]?.ToString() ?? "N/A",
                        Phone = entry.Attributes["telephoneNumber"]?[0]?.ToString() ?? "N/A",
                        Mobile = entry.Attributes["mobile"]?[0]?.ToString() ?? "N/A",
                        Manager = entry.Attributes["manager"]?[0]?.ToString() ?? "N/A"
                    };

                    // Parse lastLogon (kan være byte array)
                    if (entry.Attributes.Contains("lastLogon"))
                    {
                        var lastLogonValue = entry.Attributes["lastLogon"][0];
                        if (lastLogonValue is byte[] lastLogonBytes && lastLogonBytes.Length == 8)
                        {
                            var ticks = BitConverter.ToInt64(lastLogonBytes, 0);
                            if (ticks > 0)
                            {
                                user.LastLogon = DateTime.FromFileTime(ticks);
                            }
                        }
                    }

                    // Parse passwordLastSet
                    if (entry.Attributes.Contains("pwdLastSet"))
                    {
                        var pwdLastSetValue = entry.Attributes["pwdLastSet"][0];
                        if (pwdLastSetValue is byte[] pwdLastSetBytes && pwdLastSetBytes.Length == 8)
                        {
                            var ticks = BitConverter.ToInt64(pwdLastSetBytes, 0);
                            if (ticks > 0)
                            {
                                user.PasswordLastSet = DateTime.FromFileTime(ticks);
                            }
                        }
                    }

                    // Parse userAccountControl for enabled status
                    if (entry.Attributes.Contains("userAccountControl"))
                    {
                        var uacValue = entry.Attributes["userAccountControl"][0]?.ToString();
                        if (int.TryParse(uacValue, out int uac))
                        {
                            // Bit 2 (0x0002) = ACCOUNTDISABLE
                            user.IsEnabled = (uac & 0x0002) == 0;
                        }
                    }

                    return user;
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af brugeroplysninger: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Henter alle grupper som den aktuelle bruger er medlem af
        /// </summary>
        /// <returns>Liste af gruppenavne</returns>
        public List<string> GetCurrentUserGroups()
        {
            var groups = new List<string>();

            using (var connection = GetConnection())
            {
                // Først find brugerens distinguished name
                var userSearchRequest = new SearchRequest(
                    GetBaseDN(),
                    $"(samAccountName={_config.Username})",
                    SearchScope.Subtree,
                    "distinguishedName"
                );

                try
                {
                    var userResponse = (SearchResponse)connection.SendRequest(userSearchRequest);
                    
                    if (userResponse.Entries.Count == 0)
                    {
                        Console.WriteLine("Bruger ikke fundet for gruppeopslag");
                        return groups;
                    }

                    var userDN = userResponse.Entries[0].Attributes["distinguishedName"][0]?.ToString();
                    if (string.IsNullOrEmpty(userDN))
                    {
                        Console.WriteLine("Kunne ikke finde brugerens DN");
                        return groups;
                    }

                    // Søg efter grupper hvor brugeren er medlem
                    var groupSearchRequest = new SearchRequest(
                        GetBaseDN(),
                        $"(member={userDN})",
                        SearchScope.Subtree,
                        "cn",
                        "description"
                    );

                    var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

                    foreach (SearchResultEntry entry in groupResponse.Entries)
                    {
                        if (entry.Attributes.Contains("cn"))
                        {
                            var groupName = entry.Attributes["cn"][0]?.ToString();
                            if (!string.IsNullOrEmpty(groupName))
                            {
                                groups.Add(groupName);
                            }
                        }
                    }

                    // Prøv også med recursive group membership (hvis understøttet)
                    try
                    {
                        var recursiveGroupSearchRequest = new SearchRequest(
                            GetBaseDN(),
                            $"(member:1.2.840.113556.1.4.1941:={userDN})",
                            SearchScope.Subtree,
                            "cn"
                        );

                        var recursiveResponse = (SearchResponse)connection.SendRequest(recursiveGroupSearchRequest);

                        foreach (SearchResultEntry entry in recursiveResponse.Entries)
                        {
                            if (entry.Attributes.Contains("cn"))
                            {
                                var groupName = entry.Attributes["cn"][0]?.ToString();
                                if (!string.IsNullOrEmpty(groupName) && !groups.Contains(groupName))
                                {
                                    groups.Add(groupName);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignorer fejl for recursive search - det er ikke altid understøttet
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af brugerens grupper: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Viser detaljerede oplysninger om den aktuelle bruger (/me kommando)
        /// </summary>
        public void ShowCurrentUserInfo()
        {
            Console.WriteLine("=== Min Bruger Information (/me) ===");
            Console.WriteLine();

            try
            {
                var user = GetCurrentUserDetails();
                Console.WriteLine("Henter brugerens roller/grupper...");
                var groups = GetCurrentUserGroups();
                Console.WriteLine($"Fundet {groups.Count} roller/grupper");

                // Grundlæggende information
                Console.WriteLine("👤 GRUNDLÆGGENDE INFORMATION");
                Console.WriteLine($"   Navn: {user.DisplayName}");
                Console.WriteLine($"   Brugernavn: {user.Username}");
                Console.WriteLine($"   Email: {user.Email}");
                Console.WriteLine($"   Firma: {user.Company}");
                Console.WriteLine($"   Afdeling: {user.Department}");
                Console.WriteLine($"   Titel: {user.Title}");
                Console.WriteLine($"   Kontor: {user.Office}");
                Console.WriteLine();

                // Kontakt information
                Console.WriteLine("📞 KONTAKT INFORMATION");
                Console.WriteLine($"   Telefon: {user.Phone}");
                Console.WriteLine($"   Mobil: {user.Mobile}");
                Console.WriteLine($"   Manager: {user.Manager}");
                Console.WriteLine();

                // Konto status
                Console.WriteLine("🔐 KONTO STATUS");
                Console.WriteLine($"   Status: {(user.IsEnabled ? "✅ Aktiv" : "❌ Deaktiveret")}");
                if (user.LastLogon.HasValue)
                {
                    Console.WriteLine($"   Sidste login: {user.LastLogon.Value:dd/MM/yyyy HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine("   Sidste login: Aldrig");
                }
                if (user.PasswordLastSet.HasValue)
                {
                    Console.WriteLine($"   Adgangskode ændret: {user.PasswordLastSet.Value:dd/MM/yyyy HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine("   Adgangskode ændret: Ukendt");
                }
                Console.WriteLine();

                // Grupper/Roller
                Console.WriteLine($"👥 ROLLER/GRUPPER ({groups.Count})");
                if (groups.Count > 0)
                {
                    foreach (var group in groups.OrderBy(g => g))
                    {
                        Console.WriteLine($"   • {group}");
                    }
                }
                else
                {
                    Console.WriteLine("   Ingen roller/grupper fundet");
                }
                Console.WriteLine();

                // Tekniske detaljer
                Console.WriteLine("🔧 TEKNISKE DETALJER");
                Console.WriteLine($"   Distinguished Name: {user.DistinguishedName}");
                Console.WriteLine($"   Forbindelse: {_config.Server}");
                Console.WriteLine($"   Domæne: {_config.Domain}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fejl ved hentning af brugeroplysninger: {ex.Message}");
            }
        }
    }
}

