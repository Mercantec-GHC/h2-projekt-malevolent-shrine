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

        public async Task<List<HotelNamesDto>> GetHotelNamesAsync()
        {
            try
            {
                var hotels = await GetAllHotels();
                return hotels
                .Select(h => new HotelNamesDto { Id = h.Id, HotelName = h.Name })
                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new List<HotelNamesDto>();
            }
        }
    }
}
