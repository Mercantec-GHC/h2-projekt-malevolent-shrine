using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazor.Models;

namespace Blazor.Services
{
    public partial class APIService
    {
        private async Task EnsureAuthAsync()
        {
            var token = await GetToken();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<TicketRead?> CreateTicketAsync(TicketCreate dto)
        {
            await EnsureAuthAsync();
            var res = await _httpClient.PostAsJsonAsync("api/tickets", dto);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TicketRead>();
        }

        public async Task<List<TicketRead>> GetMyTicketsAsync()
        {
            await EnsureAuthAsync();
            var res = await _httpClient.GetAsync("api/tickets/mine");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<TicketRead>>() ?? new();
        }

        public async Task<List<TicketRead>> GetTicketsForMyRoleAsync()
        {
            await EnsureAuthAsync();
            var res = await _httpClient.GetAsync("api/tickets/for-role");
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<List<TicketRead>>() ?? new();
        }

        public async Task<TicketWithMessages?> GetTicketWithMessagesAsync(int id)
        {
            await EnsureAuthAsync();
            var res = await _httpClient.GetAsync($"api/tickets/{id}");
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TicketWithMessages>();
        }

        public async Task<TicketRead?> AssignTicketAsync(int id)
        {
            await EnsureAuthAsync();
            var res = await _httpClient.PostAsync($"api/tickets/{id}/assign", null);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TicketRead>();
        }

        public async Task<TicketRead?> SetTicketStatusAsync(int id, TicketStatus status)
        {
            await EnsureAuthAsync();
            var res = await _httpClient.PostAsJsonAsync($"api/tickets/{id}/status", new { Status = status });
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TicketRead>();
        }

        public async Task<TicketMessageRead?> AddTicketMessageAsync(int ticketId, string content)
        {
            await EnsureAuthAsync();
            var res = await _httpClient.PostAsJsonAsync("api/tickets/messages", new TicketMessageCreate { TicketId = ticketId, Content = content });
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TicketMessageRead>();
        }
    }
}
