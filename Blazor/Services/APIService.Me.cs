using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<T?> GetMeAsync<T>() where T : class
        {
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var resp = await _httpClient.GetAsync("api/Users/me");
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<T>();
            }
            catch
            {
                return null;
            }
        }
    }
}
