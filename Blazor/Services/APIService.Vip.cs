using System.Net.Http;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        // Return true if room is VIP (endpoint exists), else false
        public async Task<bool> IsVipRoom(int roomId)
        {
            try
            {
                var resp = await _httpClient.GetAsync($"api/VipRooms/{roomId}");
                return resp.IsSuccessStatusCode; // 200 -> VIP, 404 -> not VIP
            }
            catch
            {
                return false;
            }
        }
    }
}
