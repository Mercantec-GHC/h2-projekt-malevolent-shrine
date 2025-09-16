using DomainModels.DTOs;
using DomainModels.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks; 
namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<User> GetCurrentUserAsync(string? token = null)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.GetAsync("api/Users/me");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<User>()
                   ?? new User();
        }

    }
}
