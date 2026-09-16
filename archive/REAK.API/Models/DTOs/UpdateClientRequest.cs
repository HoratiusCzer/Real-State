using System.ComponentModel.DataAnnotations;
using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class UpdateClientRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    public LeadSource Source { get; set; }

    [Range(0, 100)]
    public int LeadScore { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [MaxLength(500)]
    public string? Preferences { get; set; }
}
