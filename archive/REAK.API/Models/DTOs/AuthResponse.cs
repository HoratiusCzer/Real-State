using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class AuthResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
