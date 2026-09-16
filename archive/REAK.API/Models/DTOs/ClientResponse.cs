using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class ClientResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public LeadSource Source { get; set; }
    public int LeadScore { get; set; }
    public string? Notes { get; set; }
    public string? CompanyName { get; set; }
    public string? Preferences { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LeadCount { get; set; }
    public int DealCount { get; set; }
}
