using System.DirectoryServices.Protocols;

namespace API.AD
{
    public partial class ActiveDirectoryService
    {
        /// <summary>
        /// Henter alle grupper fra Active Directory
        /// </summary>
        /// <returns>Liste af ADGroup objekter</returns>
        public List<ADGroup> GetAllGroups()
        {
            var groups = new List<ADGroup>();

            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    "(objectClass=group)",
                    SearchScope.Subtree,
                    "cn",
                    "description"
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

                        groups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af grupper: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Henter alle grupper med deres medlemmer fra Active Directory
        /// </summary>
        /// <returns>Liste af ADGroup objekter med medlemmer</returns>
        public List<ADGroup> GetAllGroupsWithMembers()
        {
            var groups = new List<ADGroup>();

            using (var connection = GetConnection())
            {
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    "(objectClass=group)",
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
                            foreach (var memberValue in entry.Attributes["member"])
                            {
                                // Konverter til string uanset om det er string eller byte array
                                string member = memberValue switch
                                {
                                    string str => str,
                                    byte[] bytes => System.Text.Encoding.UTF8.GetString(bytes),
                                    _ => memberValue?.ToString() ?? "Unknown"
                                };
                                group.Members.Add(member);
                            }
                        }

                        groups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved hentning af grupper med medlemmer: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Søger efter grupper baseret på søgeterm
        /// </summary>
        /// <param name="searchTerm">Søgeterm til at søge efter</param>
        /// <returns>Liste af matchende ADGroup objekter</returns>
        public List<ADGroup> SearchGroups(string searchTerm)
        {
            var groups = new List<ADGroup>();

            using (var connection = GetConnection())
            {
                var filter = $"(|(cn=*{searchTerm}*)(description=*{searchTerm}*))";
                
                var searchRequest = new SearchRequest(
                    GetBaseDN(),
                    filter,
                    SearchScope.Subtree,
                    "cn",
                    "description"
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

                        groups.Add(group);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fejl ved søgning efter grupper: {ex.Message}");
                }
            }

            return groups;
        }

        /// <summary>
        /// Viser alle grupper i konsollen
        /// </summary>
        public void ShowAllGroups()
        {
            Console.WriteLine("=== Alle Grupper ===");
            var groups = GetAllGroups();
            
            if (groups.Count == 0)
            {
                Console.WriteLine("Ingen grupper fundet.");
                return;
            }

            Console.WriteLine($"Fundet {groups.Count} grupper:");
            Console.WriteLine();
            
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                Console.WriteLine($"Navn: {group.Name}");
                Console.WriteLine($"Beskrivelse: {group.Description}");
                Console.WriteLine(new string('-', 50));
            }
        }

        /// <summary>
        /// Viser alle grupper med deres medlemmer i konsollen
        /// </summary>
        public void ShowGroupsWithMembers()
        {
            Console.WriteLine("=== Grupper med Medlemmer ===");
            var groups = GetAllGroupsWithMembers();
            
            if (groups.Count == 0)
            {
                Console.WriteLine("Ingen grupper fundet.");
                return;
            }

            Console.WriteLine($"Fundet {groups.Count} grupper:");
            Console.WriteLine();
            
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                Console.WriteLine($"Gruppe: {group.Name}");
                Console.WriteLine($"Beskrivelse: {group.Description}");
                Console.WriteLine($"Medlemmer ({group.Members.Count}):");
                
                if (group.Members.Count > 0)
                {
                    foreach (var member in group.Members.OrderBy(m => m))
                    {
                        Console.WriteLine($"  - {member}");
                    }
                }
                else
                {
                    Console.WriteLine("  Ingen medlemmer");
                }
                
                Console.WriteLine(new string('-', 50));
            }
        }

        /// <summary>
        /// Søger og viser grupper baseret på søgeterm
        /// </summary>
        public void SearchGroups()
        {
            Console.WriteLine("=== Søg efter Grupper ===");
            Console.Write("Indtast søgekriterie (navn eller beskrivelse): ");
            var searchTerm = Console.ReadLine();

            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine("Søgeterm kan ikke være tom.");
                return;
            }

            var groups = SearchGroups(searchTerm);
            
            if (groups.Count == 0)
            {
                Console.WriteLine("Ingen grupper fundet med det søgekriterie.");
                return;
            }

            Console.WriteLine($"Fundet {groups.Count} grupper:");
            Console.WriteLine();
            
            foreach (var group in groups.OrderBy(g => g.Name))
            {
                Console.WriteLine($"Navn: {group.Name}");
                Console.WriteLine($"Beskrivelse: {group.Description}");
                Console.WriteLine(new string('-', 50));
            }
        }
    }
}

