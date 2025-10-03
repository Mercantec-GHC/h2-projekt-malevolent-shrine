using System.DirectoryServices.Protocols;

namespace API.AD
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Avanceret søgning efter brugere med flere kriterier
        /// </summary>
        /// <param name="name">Navn søgekriterie</param>
        /// <param name="email">Email søgekriterie</param>
        /// <param name="department">Afdeling søgekriterie</param>
        /// <param name="title">Titel søgekriterie</param>
        /// <returns>Liste af matchende ADUser objekter</returns>
        public List<ADUser> AdvancedUserSearch(string? name = null, string? email = null, string? department = null, string? title = null)
        {
            var users = new List<ADUser>();
            var searchConditions = new List<string>();

            // Byg søgefilter baseret på de angivne kriterier
            if (!string.IsNullOrEmpty(name))
                searchConditions.Add($"(cn=*{name}*)");
            
            if (!string.IsNullOrEmpty(email))
                searchConditions.Add($"(mail=*{email}*)");
            
            if (!string.IsNullOrEmpty(department))
                searchConditions.Add($"(department=*{department}*)");
            
            if (!string.IsNullOrEmpty(title))
                searchConditions.Add($"(title=*{title}*)");

            // Hvis ingen kriterier er angivet, returner alle brugere
            if (searchConditions.Count == 0)
                return GetAllUsers();

            // Kombiner alle kriterier med AND operator
            var filter = $"(&(objectClass=user){string.Join("", searchConditions)})";

            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    filter,
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
                    throw new Exception($"Fejl ved avanceret brugersøgning: {ex.Message}");
                }
            }

            return users;
        }

        /// <summary>
        /// Avanceret søgning efter grupper med flere kriterier
        /// </summary>
        /// <param name="name">Navn søgekriterie</param>
        /// <param name="description">Beskrivelse søgekriterie</param>
        /// <param name="hasMembers">Om gruppen skal have medlemmer</param>
        /// <returns>Liste af matchende ADGroup objekter</returns>
        public List<ADGroup> AdvancedGroupSearch(string? name = null, string? description = null, bool? hasMembers = null)
        {
            var groups = new List<ADGroup>();
            var searchConditions = new List<string>();

            // Byg søgefilter baseret på de angivne kriterier
            if (!string.IsNullOrEmpty(name))
                searchConditions.Add($"(cn=*{name}*)");
            
            if (!string.IsNullOrEmpty(description))
                searchConditions.Add($"(description=*{description}*)");

            // Hvis ingen kriterier er angivet, returner alle grupper
            if (searchConditions.Count == 0)
                return GetAllGroups();

            // Kombiner alle kriterier med AND operator
            var filter = $"(&(objectClass=group){string.Join("", searchConditions)})";

            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    filter,
                    SearchScope.Subtree,
                    "cn",
                    "description",
                    "member"
                );

                try
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    foreach (SearchResultEntry entry in response.Entries)
                    {
                        var group = new ADGroup
                        {
                            Name = entry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                            Description = entry.Attributes["description"]?[0]?.ToString() ?? "N/A"
                        };

                        // Hent medlemmer
                        if (entry.Attributes.Contains("member"))
                        {
                            foreach (string member in entry.Attributes["member"])
                            {
                                group.Members.Add(member);
                            }
                        }

                        // Filtrer baseret på medlemskriterie
                        if (hasMembers.HasValue)
                        {
                            if (hasMembers.Value && group.Members.Count == 0)
                                continue;
                            if (!hasMembers.Value && group.Members.Count > 0)
                                continue;
                        }

                        groups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved avanceret gruppesøgning: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Søger efter brugere i en specifik gruppe
        /// </summary>
        /// <param name="groupName">Navn på gruppen</param>
        /// <returns>Liste af brugere i gruppen</returns>
        public List<ADUser> GetUsersInGroup(string groupName)
        {
            var users = new List<ADUser>();

            using (var connection = GetConnection())
            {
                // Først find gruppen
                var groupSearchRequest = new SearchRequest(
                    GetBaseDN(),
                    $"(cn={groupName})",
                    SearchScope.Subtree,
                    "member"
                );

                try
                {
                    var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);
                    
                    if (groupResponse.Entries.Count == 0)
                    {
                        Console.WriteLine($"Gruppe '{groupName}' ikke fundet.");
                        return users;
                    }

                    var groupEntry = groupResponse.Entries[0];
                    
                    if (!groupEntry.Attributes.Contains("member"))
                    {
                        Console.WriteLine($"Gruppe '{groupName}' har ingen medlemmer.");
                        return users;
                    }

                    // For hver medlem, hent brugeroplysninger
                    foreach (string memberDN in groupEntry.Attributes["member"])
                    {
                        var userSearchRequest = new SearchRequest(
                            memberDN,
                            "(objectClass=user)",
                            SearchScope.Base,
                            "cn",
                            "samAccountName",
                            "mail",
                            "department",
                            "title",
                            "distinguishedName"
                        );

                        try
                        {
                            var userResponse = (SearchResponse)connection.SendRequest(userSearchRequest);
                            
                            if (userResponse.Entries.Count > 0)
                            {
                                var userEntry = userResponse.Entries[0];
                                var user = new ADUser
                                {
                                    Name = userEntry.Attributes["cn"]?[0]?.ToString() ?? "N/A",
                                    Username = userEntry.Attributes["samAccountName"]?[0]?.ToString() ?? "N/A",
                                    Email = userEntry.Attributes["mail"]?[0]?.ToString() ?? "N/A",
                                    Department = userEntry.Attributes["department"]?[0]?.ToString() ?? "N/A",
                                    Title = userEntry.Attributes["title"]?[0]?.ToString() ?? "N/A",
                                    DistinguishedName = userEntry.Attributes["distinguishedName"]?[0]?.ToString() ?? "N/A"
                                };

                                users.Add(user);
                            }
                        }
                        catch
                        {
                            // Ignorer fejl for individuelle medlemmer
                            continue;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af brugere i gruppe: {ex.Message}");
                }
            }

            return users;
        }

        /// <summary>
        /// Viser avanceret søgning menu
        /// </summary>
        public void ShowAdvancedSearchMenu()
        {
            Console.WriteLine("=== Avanceret Søgning ===");
            Console.WriteLine("1. Avanceret brugersøgning");
            Console.WriteLine("2. Avanceret gruppesøgning");
            Console.WriteLine("3. Find brugere i specifik gruppe");
            Console.WriteLine("0. Tilbage til hovedmenu");
            Console.WriteLine();
            Console.Write("Vælg en option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAdvancedUserSearch();
                    break;
                case "2":
                    ShowAdvancedGroupSearch();
                    break;
                case "3":
                    ShowUsersInGroup();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Ugyldigt valg.");
                    break;
            }
        }

        private void ShowAdvancedUserSearch()
        {
            Console.WriteLine("=== Avanceret Brugersøgning ===");
            Console.WriteLine("Indtast søgekriterier (tryk Enter for at springe over):");
            Console.WriteLine();

            Console.Write("Navn: ");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name)) name = null;

            Console.Write("Email: ");
            var email = Console.ReadLine();
            if (string.IsNullOrEmpty(email)) email = null;

            Console.Write("Afdeling: ");
            var department = Console.ReadLine();
            if (string.IsNullOrEmpty(department)) department = null;

            Console.Write("Titel: ");
            var title = Console.ReadLine();
            if (string.IsNullOrEmpty(title)) title = null;

            var users = AdvancedUserSearch(name, email, department, title);
            
            if (users.Count == 0)
            {
                Console.WriteLine("Ingen brugere fundet med de angivne kriterier.");
                return;
            }

            Console.WriteLine($"\nFundet {users.Count} brugere:");
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

        private void ShowAdvancedGroupSearch()
        {
            Console.WriteLine("=== Avanceret Gruppesøgning ===");
            Console.WriteLine("Indtast søgekriterier (tryk Enter for at springe over):");
            Console.WriteLine();

            Console.Write("Navn: ");
            var name = Console.ReadLine();
            if (string.IsNullOrEmpty(name)) name = null;

            Console.Write("Beskrivelse: ");
            var description = Console.ReadLine();
            if (string.IsNullOrEmpty(description)) description = null;

            Console.Write("Skal gruppen have medlemmer? (j/n): ");
            var hasMembersInput = Console.ReadLine();
            bool? hasMembers = null;
            if (hasMembersInput?.ToLower() == "j")
                hasMembers = true;
            else if (hasMembersInput?.ToLower() == "n")
                hasMembers = false;

            var groups = AdvancedGroupSearch(name, description, hasMembers);
            
            if (groups.Count == 0)
            {
                Console.WriteLine("Ingen grupper fundet med de angivne kriterier.");
                return;
            }

            Console.WriteLine($"\nFundet {groups.Count} grupper:");
            Console.WriteLine();
            
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                Console.WriteLine($"Navn: {group.Name}");
                Console.WriteLine($"Beskrivelse: {group.Description}");
                Console.WriteLine($"Medlemmer: {group.Members.Count}");
                Console.WriteLine(new string('-', 50));
            }
        }

        private void ShowUsersInGroup()
        {
            Console.WriteLine("=== Find Brugere i Specifik Gruppe ===");
            Console.Write("Indtast gruppens navn: ");
            var groupName = Console.ReadLine();

            if (string.IsNullOrEmpty(groupName))
            {
                Console.WriteLine("Gruppenavn kan ikke være tomt.");
                return;
            }

            var users = GetUsersInGroup(groupName);
            
            if (users.Count == 0)
            {
                Console.WriteLine($"Ingen brugere fundet i gruppen '{groupName}'.");
                return;
            }

            Console.WriteLine($"\nFundet {users.Count} brugere i gruppen '{groupName}':");
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
    }
}

