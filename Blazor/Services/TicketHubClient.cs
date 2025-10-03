using Blazor.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Blazor.Services
{
    // Очень простой клиент SignalR для /hubs/tickets
    public class TicketHubClient : IAsyncDisposable
    {
        private readonly APIService _api;
        private readonly ILogger<TicketHubClient> _logger;
        private HubConnection? _conn;
        private readonly string _hubUrl;

        public event Func<TicketRead, Task>? OnTicketCreated;   // для роли
        public event Func<TicketRead, Task>? OnTicketUpdated;   // для участников тикета
        public event Func<TicketMessageRead, Task>? OnNewMessage; // для чата
        public event Func<ToastPayload, Task>? OnToast; // для тостов админке

        public TicketHubClient(APIService api, ILogger<TicketHubClient> logger)
        {
            _api = api;
            _logger = logger;
            _hubUrl = "hubs/tickets"; // относительный путь хаба
        }

        public bool IsConnected => _conn?.State == HubConnectionState.Connected;

        public async Task StartAsync(Uri apiBaseAddress)
        {
            if (_conn != null && IsConnected) return;

            var hubAbsolute = new Uri(apiBaseAddress, _hubUrl).ToString();
            _conn = new HubConnectionBuilder()
                .WithUrl(hubAbsolute, options =>
                {
                    options.AccessTokenProvider = async () => await _api.GetToken();
                })
                .WithAutomaticReconnect()
                .Build();

            _conn.On<TicketRead>("TicketCreated", async t => { if (OnTicketCreated != null) await OnTicketCreated(t); });
            _conn.On<TicketRead>("TicketUpdated", async t => { if (OnTicketUpdated != null) await OnTicketUpdated(t); });
            _conn.On<TicketMessageRead>("NewMessage", async m => { if (OnNewMessage != null) await OnNewMessage(m); });
            _conn.On<ToastPayload>("toast", async payload => { if (OnToast != null) await OnToast(payload); });

            try
            {
                await _conn.StartAsync();
                _logger.LogInformation("TicketHub connected: {State}", _conn.State);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TicketHub connect failed");
            }
        }

        public async Task JoinTicket(int ticketId)
        {
            if (_conn == null) return;
            await _conn.InvokeAsync("JoinTicket", ticketId);
        }

        public async Task LeaveTicket(int ticketId)
        {
            if (_conn == null) return;
            await _conn.InvokeAsync("LeaveTicket", ticketId);
        }

        public async Task SendMessage(int ticketId, string content)
        {
            if (_conn == null) return;
            await _conn.InvokeAsync("SendMessage", ticketId, content);
        }

        public async ValueTask DisposeAsync()
        {
            if (_conn != null)
            {
                try { await _conn.DisposeAsync(); } catch { /* ignore */ }
            }
        }
    }
}
