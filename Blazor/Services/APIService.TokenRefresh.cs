using System.Net.Http.Json;
using DomainModels.DTOs;
using Microsoft.JSInterop;

namespace Blazor.Services
{
    public partial class APIService
    {
        private const string REFRESH_TOKEN_KEY = "refresh_token";
        
        /// <summary>
        /// Сохраняет refresh token в localStorage
        /// </summary>
        public async Task SaveRefreshToken(string refreshToken)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", REFRESH_TOKEN_KEY, refreshToken);
        }
        
        /// <summary>
        /// Получает refresh token из localStorage
        /// </summary>
        public async Task<string?> GetRefreshToken()
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", REFRESH_TOKEN_KEY);
        }
        
        /// <summary>
        /// Обновляет access token используя refresh token
        /// </summary>
        public async Task<bool> RefreshAccessToken()
        {
            try
            {
                var refreshToken = await GetRefreshToken();
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return false;

                var response = await _httpClient.PostAsJsonAsync("api/Auth/refresh-token", new RefreshTokenDto
                {
                    RefreshToken = refreshToken
                });

                if (!response.IsSuccessStatusCode)
                {
                    // Refresh token истёк или недействителен - выходим
                    await RevokeTokens();
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                if (result == null || string.IsNullOrEmpty(result.AccessToken))
                    return false;

                // Сохраняем новые токены
                await _js.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, result.AccessToken);
                await SaveRefreshToken(result.RefreshToken);

                // ВАЖНО: Обновляем Authorization заголовок с новым токеном
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

                Console.WriteLine("✅ Access token refreshed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Refresh token error: {ex.Message}");
                await RevokeTokens();
                return false;
            }
        }
        
        /// <summary>
        /// Отзывает (удаляет) все токены - полный logout
        /// </summary>
        public async Task RevokeTokens()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
            await _js.InvokeVoidAsync("localStorage.removeItem", REFRESH_TOKEN_KEY);
            Console.WriteLine("🔒 Tokens revoked");
        }
        
        /// <summary>
        /// Проверяет, нужно ли обновить токен (вызывать перед важными запросами)
        /// </summary>
        public async Task<bool> EnsureValidToken()
        {
            var token = await GetToken();
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Простая проверка: если токен есть, пробуем использовать
            // Если получим 401 - автоматически обновим через RefreshAccessToken
            return true;
        }
        
        /// <summary>
        /// Простой wrapper: выполняет запрос, если 401 - обновляет токен и пробует еще раз
        /// </summary>
        public async Task<HttpResponseMessage> TryWithTokenRefresh(Func<Task<HttpResponseMessage>> request)
        {
            var response = await request();
            
            // Если не 401 - возвращаем как есть
            if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                return response;
            
            // Пробуем обновить токен
            var refreshed = await RefreshAccessToken();
            if (!refreshed)
            {
                Console.WriteLine("❌ Token refresh failed, logout required");
                return response; // возвращаем 401
            }
            
            // Повторяем запрос с новым токеном
            Console.WriteLine("🔄 Retrying request with new token");
            return await request();
        }
    }
}
