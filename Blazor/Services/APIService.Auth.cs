using System.Net.Http.Json;
using DomainModels.DTOs;
using DomainModels.Models;
using Microsoft.JSInterop;

namespace Blazor.Services
{
    public partial class APIService
    {
        private const string TOKEN_KEY = "access_token";

        public async Task<bool> Register(AuthDto user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", user);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Login(UserLoginDto user)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", user);

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result?.Token == null)
                return false;

            await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, result.Token);

            return true;
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
