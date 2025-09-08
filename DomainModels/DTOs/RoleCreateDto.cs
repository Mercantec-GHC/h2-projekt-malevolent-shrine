namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class RoleCreateDto
{
    [Required]
    [StringLength(50)]
    public required string Name { get; set; }
}