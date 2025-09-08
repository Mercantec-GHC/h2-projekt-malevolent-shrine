using DomainModels.DTOs;
using DomainModels.Models;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<List<Hotel>> GetAllHotels()
        {
            try
            {
                return await httpClient.GetFromJsonAsync<List<Hotel>>("api/Hotels");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<Hotel>();
            }
        }
    }
}
