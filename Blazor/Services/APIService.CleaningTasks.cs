// filepath: /Users/deuswork/RiderProjects/h2-projekt-malevolent-shrine/Blazor/Services/APIService.CleaningTasks.cs
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DomainModels.DTOs;

namespace Blazor.Services
{
    public partial class APIService
    {
        // Local DTOs for cleaning tasks to avoid coupling
        public enum CleaningTaskStatus { New = 0, InProgress = 1, Done = 2 }
        public class CleaningTaskCreateDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int? RoomId { get; set; }
            public int AssignedToUserId { get; set; }
            public DateTime? DueDate { get; set; }
        }
        public class CleaningTaskReadDto
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public int? RoomId { get; set; }
            public int AssignedToUserId { get; set; }
            public int CreatedByUserId { get; set; }
            public DateTime? DueDate { get; set; }
            public CleaningTaskStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        public class CleaningTaskStatusUpdateDto
        {
            public CleaningTaskStatus Status { get; set; }
        }

        private void EnsureBearer(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<UserReadDto>> GetUsersByRoleAsync(string roleName, string token)
        {
            EnsureBearer(token);
            var resp = await _httpClient.GetAsync($"api/Users/by-role/{Uri.EscapeDataString(roleName)}");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<UserReadDto>>() ?? new();
        }

        public async Task<List<CleaningTaskReadDto>> GetMyCleaningTasksAsync(string token)
        {
            EnsureBearer(token);
            var resp = await _httpClient.GetAsync("api/cleaningtasks/my");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<CleaningTaskReadDto>>() ?? new();
        }

        public async Task<bool> CreateCleaningTaskAsync(CleaningTaskCreateDto dto, string token)
        {
            EnsureBearer(token);
            var resp = await _httpClient.PostAsJsonAsync("api/cleaningtasks", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateMyCleaningTaskStatusAsync(int id, CleaningTaskStatus status, string token)
        {
            EnsureBearer(token);
            var payload = new CleaningTaskStatusUpdateDto { Status = status };
            var resp = await _httpClient.PutAsJsonAsync($"api/cleaningtasks/{id}/status", payload);
            return resp.IsSuccessStatusCode;
        }
    }
}
