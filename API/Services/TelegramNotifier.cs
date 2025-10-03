using System.Net.Http;
using Microsoft.Extensions.Configuration;
using API.Models;

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
                // не бросаем исключение, чтобы не мешать бизнес-процессу
            }
            catch
            {
                // проглатываем ошибки Telegram
            }
        }

        // -------------------- Форматирование шаблонов --------------------
        // locale: "ru" (по умолчанию) или "en". При необходимости можно расширить.
        public string FormatTicketCreated(int id, string title, TicketCategory category, string targetRole, string locale = "ru")
        {
            if (string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase))
            {
                return $"🆕 Ticket #{id} — <b>{Escape(title)}</b> ({category}) for role <b>{Escape(targetRole)}</b>";
            }
            // RU по умолчанию + ведущие нули в номере (читается приятнее)
            return $"🆕 Тикет №{id:D4} — <b>{Escape(title)}</b> ({category}) для роли <b>{Escape(targetRole)}</b>";
        }

        public string FormatTicketStatus(int id, TicketStatus status, string locale = "ru")
        {
            if (string.Equals(locale, "en", StringComparison.OrdinalIgnoreCase))
            {
                return $"✅ Ticket #{id} status: <b>{status}</b>";
            }
            return $"✅ Тикет №{id:D4} — статус: <b>{status}</b>";
        }

        private static string Escape(string s)
        {
            // Лёгкая экранизация для HTML в Telegram (минимально достаточная)
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
