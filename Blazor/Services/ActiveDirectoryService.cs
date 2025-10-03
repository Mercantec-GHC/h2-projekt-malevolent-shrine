using System.Net.Http.Json;
using System.Text.Json;
using Blazor.Models;
using Microsoft.JSInterop;

namespace Blazor.Services
{
    public class ActiveDirectoryService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;
        private const string TOKEN_KEY = "access_token";

        public ActiveDirectoryService(HttpClient httpClient, IJSRuntime js)
        {
            _httpClient = httpClient;
            _js = js;
        }

        /// <summary>
        /// Добавляет JWT токен к запросу для авторизации
        /// </summary>
        private async Task AddAuthorizationHeaderAsync()
        {
            var token = await _js.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        /// <summary>
        /// Получить всех пользователей из AD
        /// </summary>
        public async Task<List<ADUser>> GetAllUsersAsync()
        {
            try
            {
                await AddAuthorizationHeaderAsync();
                var users = await _httpClient.GetFromJsonAsync<List<ADUser>>("api/activedirectory/users");
                return users ?? new List<ADUser>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения пользователей: {ex.Message}");
                return new List<ADUser>();
            }
        }

        /// <summary>
        /// Поиск пользователей по имени
        /// </summary>
        public async Task<List<ADUser>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                await AddAuthorizationHeaderAsync();
                var users = await _httpClient.GetFromJsonAsync<List<ADUser>>($"api/activedirectory/users/search/{Uri.EscapeDataString(searchTerm)}");
                return users ?? new List<ADUser>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка поиска пользователей: {ex.Message}");
                return new List<ADUser>();
            }
        }

        /// <summary>
        /// Получить все группы из AD
        /// </summary>
        public async Task<List<ADGroup>> GetAllGroupsAsync()
        {
            try
            {
                await AddAuthorizationHeaderAsync();
                var groups = await _httpClient.GetFromJsonAsync<List<ADGroup>>("api/activedirectory/groups");
                return groups ?? new List<ADGroup>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения групп: {ex.Message}");
                return new List<ADGroup>();
            }
        }

        /// <summary>
        /// Тест соединения с AD
        /// </summary>
        public async Task<(bool Success, string Message)> TestConnectionAsync()
        {
            try
            {
                await AddAuthorizationHeaderAsync();
                var response = await _httpClient.PostAsJsonAsync("api/activedirectory/test-connection", new { });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ConnectionTestResponse>();
                    return (true, result?.Message ?? "Соединение успешно");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return (false, $"Ошибка: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Исключение: {ex.Message}");
            }
        }
    }
}
