namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class RoleUpdateDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
}