namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to create a new user.
/// </summary>
public class UserCreateDto
{
    /// <summary>
    /// Given name.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }
    
    /// <summary>
    /// Family name (surname).
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }
    
    /// <summary>
    /// Primary e-mail address.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Email { get; set; }
    
    /// <summary>
    /// Contact phone number.
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }
    
    /// <summary>
    /// Unique username.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Username { get; set; }
    
    /// <summary>
    /// Plain-text password to set for the account.
    /// </summary>
    [Required]
    [MinLength(6)]
    [StringLength(100)]
    public required string Password { get; set; }
    
    /// <summary>
    /// Optional mailing address.
    /// </summary>
    [StringLength(200)]
    public string? Address { get; set; }
}