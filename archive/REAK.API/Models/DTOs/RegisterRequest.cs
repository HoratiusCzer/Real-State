using System.ComponentModel.DataAnnotations;
using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; }

    [MaxLength(255)]
    public string? FullName { get; set; }

    [Phone(ErrorMessage = "Invalid phone format")]
    [MaxLength(50)]
    public string? Phone { get; set; }
}
