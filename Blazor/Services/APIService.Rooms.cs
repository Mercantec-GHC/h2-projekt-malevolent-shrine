using System.Net.Http.Json;
using Blazor.Models;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<List<RoomAvailabilityVM>> GetRoomAvailabilityAsync(int days = 28, int? hotelId = null)
        {
            try
            {
                var url = $"api/rooms/availability?days={days}" + (hotelId.HasValue ? $"&hotelId={hotelId.Value}" : "");
                var res = await _httpClient.GetFromJsonAsync<List<RoomAvailabilityVM>>(url);
                return res ?? new List<RoomAvailabilityVM>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Availability error: {ex.Message}");
                return new List<RoomAvailabilityVM>();
            }
        }
    }
}

