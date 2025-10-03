using System.Net.Http.Json;
using DomainModels.DTOs;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<List<RoomReadDto>> GetAllRoomsAsync()
        {
            try
            {
                var rooms = await _httpClient.GetFromJsonAsync<List<RoomReadDto>>("api/Rooms");
                return rooms ?? new List<RoomReadDto>();
            }
            catch
            {
                return new List<RoomReadDto>();
            }
        }
    }
}
