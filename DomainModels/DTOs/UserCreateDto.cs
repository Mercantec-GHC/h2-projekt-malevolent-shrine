namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class UserCreateDto
{
    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string? LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [Required]
    [MinLength(6)]
    [StringLength(100)]
    public string? Password { get; set; }

    [Required]
    public string? Username { get; set; }


    [StringLength(200)]
    public string? Address { get; set; }
}