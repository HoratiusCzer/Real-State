using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [Required]
    public LeadSource Source { get; set; }

    [Range(0, 100)]
    public int LeadScore { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [MaxLength(500)]
    public string? Preferences { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
