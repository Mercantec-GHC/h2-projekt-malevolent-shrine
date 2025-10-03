namespace Blazor.Models
{
    public class ToastPayload
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "info"; // info|success|warning|error
        public DateTime Ts { get; set; } = DateTime.UtcNow;
    }
}

