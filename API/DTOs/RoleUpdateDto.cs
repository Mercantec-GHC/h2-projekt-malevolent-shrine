namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents the payload to update an existing role.
/// </summary>
public class RoleUpdateDto
{
    /// <summary>
    /// Role identifier.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Updated role name.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
}