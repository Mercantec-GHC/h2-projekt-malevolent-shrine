using System.Net.Http.Json;
using DomainModels.DTOs;
using DomainModels.Models;
using Microsoft.JSInterop;

namespace Blazor.Services
{
    public partial class APIService
    {
        private const string TOKEN_KEY = "access_token";

        public async Task<bool> RegisterAsync(AuthDto user)
        {

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", user);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> Login(UserLoginDto user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", user);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Login failed: {response.StatusCode} - {errorContent}");
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();

                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                    return false;

                await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, result.AccessToken);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login exception: {ex.Message}");
                return false;
            }
        }

        public async Task<string?> GetToken()
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", TOKEN_KEY);
        }

        public async Task Logout()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
        }
    }

    public class LoginResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
