using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace Blazor.Services
{
    // Client helper to call AD login endpoint and store JWT in localStorage
    public class AdAuthClientService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;
        private const string TOKEN_KEY = "access_token"; // matches existing usage

        public AdAuthClientService(HttpClient http, IJSRuntime js)
        {
            _http = http; _js = js;
        }

        public async Task<(bool Success, string? Error)> LoginWithAdAsync(string username, string password)
        {
            var payload = new AdLoginRequestDto { Username = username, Password = password };
            using var response = await _http.PostAsJsonAsync("api/adauth/login", payload);
            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var errObj = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    var msg = errObj?.error ?? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                    return (false, msg);
                }
                catch
                {
                    return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }

            var result = await response.Content.ReadFromJsonAsync<AdLoginResponseDto>();
            if (result == null || string.IsNullOrEmpty(result.AccessToken))
            {
                return (false, "Пустой ответ от сервера при логине AD.");
            }

            await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, result.AccessToken);
            return (true, null);
        }

        private class ErrorResponse { public string? error { get; set; } }

        // Local DTOs to avoid referencing server-side namespaces
        private class AdLoginRequestDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        private class AdLoginResponseDto
        {
            public string Message { get; set; } = string.Empty;
            public string AccessToken { get; set; } = string.Empty;
            public DateTime AccessTokenExpiry { get; set; }
            public string? Email { get; set; }
            public string? DisplayName { get; set; }
            public string? Username { get; set; }
            public List<string> AdGroups { get; set; } = new();
            public string? AppRole { get; set; }
        }
    }
}