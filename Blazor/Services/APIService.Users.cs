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
        public async Task<UserReadDto> GetCurrentUserAsync(string? token = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return await _httpClient.GetFromJsonAsync<UserReadDto>("api/Users/me");
            } catch (Exception ex)
            {
                Console.WriteLine($"{ex} {ex.Message} {ex.StackTrace}");
                return new UserReadDto();
            }
        }
    }
}
