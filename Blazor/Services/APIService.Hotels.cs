using DomainModels.DTOs;
using DomainModels.Models;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<List<HotelReadDto>> GetAllHotels()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<HotelReadDto>>("api/Hotels");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<HotelReadDto>();
            }
        }
    }
}
