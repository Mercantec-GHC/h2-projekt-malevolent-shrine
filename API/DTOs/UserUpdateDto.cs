namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to update an existing user.
/// </summary>
public class UserUpdateDto
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// User's given name.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }
    
    /// <summary>
    /// User's family name (surname).
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }
    
    /// <summary>
    /// Primary e-mail address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public required string Email { get; set; }
    
    /// <summary>
    /// Contact phone number.
    /// </summary>
    [Required]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }
    
    /// <remarks>
    /// Password updates are handled via a dedicated endpoint and are not part of this DTO.
    /// </remarks>
    [StringLength(200)]
    public string? Address { get; set; }

    /// <summary>
    /// Username used for login; must be unique.
    /// </summary>
    public string Username { get; set; } = string.Empty;
}