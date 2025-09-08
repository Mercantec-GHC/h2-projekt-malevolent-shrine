namespace API.DTOs;
using System.ComponentModel.DataAnnotations;

public class UserCreateDto
{
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Email { get; set; }
    
    [Required]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Username { get; set; }
    
    [Required]
    [MinLength(6)]
    [StringLength(100)]
    public required string Password { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
}