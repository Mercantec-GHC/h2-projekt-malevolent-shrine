namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to create a new role.
/// </summary>
public class RoleCreateDto
{
    /// <summary>
    /// Name of the role.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
}