using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace API.Models
{
    public class RefreshToken : Common
    {
        // Храним хеш токена в БД
        public string TokenHash { get; set; } = string.Empty;

        // Для удобства работы с API - не сохраняется в БД
        [NotMapped]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string? ReplacedByToken { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? RevokedReason { get; set; } // Новое поле для причины отзыва

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;

        // Метод для вычисления хеша токена
        public static string ComputeTokenHash(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        // Метод для установки токена (автоматически вычисляет хеш)
        public void SetToken(string token)
        {
            Token = token;
            TokenHash = ComputeTokenHash(token);
        }
    }
}