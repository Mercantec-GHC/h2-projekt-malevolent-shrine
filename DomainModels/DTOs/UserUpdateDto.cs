namespace DomainModels.DTOs;
using System.ComponentModel.DataAnnotations;

public class UserUpdateDto
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public required string Email { get; set; }
    
    [Required]
    [StringLength(20)]
    public required string PhoneNumber { get; set; }
    
    // Пароль мы будем менять отдельно, поэтому здесь его не будет
    
    [StringLength(200)]
    public string? Address { get; set; }
}