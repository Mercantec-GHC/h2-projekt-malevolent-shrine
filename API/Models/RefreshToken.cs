using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace API.Models
{
    public class RefreshToken : Common
    {
        // ������ ��� ������ � ��
        public string TokenHash { get; set; } = string.Empty;

        // ��� �������� ������ � API - �� ����������� � ��
        [NotMapped]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string? ReplacedByToken { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? RevokedReason { get; set; } // ����� ���� ��� ������� ������

        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public bool IsActive => !IsRevoked && !IsExpired;

        // ����� ��� ���������� ���� ������
        public static string ComputeTokenHash(string token)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        // ����� ��� ��������� ������ (������������� ��������� ���)
        public void SetToken(string token)
        {
            Token = token;
            TokenHash = ComputeTokenHash(token);
        }
    }
}