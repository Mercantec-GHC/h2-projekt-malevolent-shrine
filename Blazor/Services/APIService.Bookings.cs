using System.Net.Http.Headers;
using System.Net.Http.Json;
using DomainModels.DTOs;

namespace Blazor.Services
{
    public partial class APIService
    {
        public async Task<RoomReadDto?> GetRoomAsync(int roomId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RoomReadDto>($"api/Rooms/{roomId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> CreateBookingAsync(BookingCreateDto dto)
        {
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var resp = await _httpClient.PostAsJsonAsync("api/Bookings", dto);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool ok, string? message)> CreateBookingWithMessageAsync(BookingCreateDto dto)
        {
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var resp = await _httpClient.PostAsJsonAsync("api/Bookings", dto);
                if (resp.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                var reason = await resp.Content.ReadAsStringAsync();
                // удобочитаемое сообщение по коду
                if ((int)resp.StatusCode == 401)
                    return (false, "You must login.");
                if ((int)resp.StatusCode == 403)
                    return (false, "Forbidden.");
                if ((int)resp.StatusCode == 400)
                    return (false, string.IsNullOrWhiteSpace(reason) ? "Bad request." : reason);
                return (false, string.IsNullOrWhiteSpace(reason) ? $"Error {(int)resp.StatusCode}." : reason);
            }
            catch (Exception ex)
            {
                return (false, $"Network error: {ex.Message}");
            }
        }

        public async Task<List<BookingReadDto>> GetMyBookingsAsync()
        {
            var list = new List<BookingReadDto>();
            try
            {
                var token = await GetToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var resp = await _httpClient.GetAsync("api/Bookings/my");
                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadFromJsonAsync<List<BookingReadDto>>();
                    if (data != null) list = data;
                }
            }
            catch { }
            return list;
        }
    }
}