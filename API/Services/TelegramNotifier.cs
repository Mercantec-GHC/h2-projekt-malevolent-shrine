using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace API.Services
{
    // Простой нотификатор в Telegram. Настройте в appsettings:
    // "Telegram": { "BotToken": "...", "ChatId": "..." }
    public class TelegramNotifier
    {
        private readonly HttpClient _http;
        private readonly string? _token;
        private readonly string? _chatId;

        public TelegramNotifier(IConfiguration cfg, IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
            _token = cfg["Telegram:BotToken"] ?? Environment.GetEnvironmentVariable("TELEGRAM__BotToken");
            _chatId = cfg["Telegram:ChatId"] ?? Environment.GetEnvironmentVariable("TELEGRAM__ChatId");
        }

        public async Task SendAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_token) || string.IsNullOrWhiteSpace(_chatId))
            {
                // Не настроено — тихо выходим (минималистично, без падения сервера)
                return;
            }

            var url = $"https://api.telegram.org/bot{_token}/sendMessage";
            using var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("chat_id", _chatId!),
                new KeyValuePair<string,string>("text", text),
                new KeyValuePair<string,string>("parse_mode", "HTML"),
                new KeyValuePair<string,string>("disable_web_page_preview", "true")
            });

            try
            {
                var resp = await _http.PostAsync(url, content);
                // не бросаем исключение, чтобы не ломать основной поток
            }
            catch
            {
                // проглатываем ошибки Telegram, чтобы не мешать бизнес-процессу
            }
        }
    }
}

