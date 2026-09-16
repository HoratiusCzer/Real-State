using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Entities.Demands;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>A member real estate company. The organization-level tenancy boundary that RLS scopes
/// most private data (listings, demands, contacts) to.</summary>
public class MemberEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public string? Website { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    /// <summary>False means the organization is suspended — every member of it loses access (spec §2.2, §17).</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<EntityUser> EntityUsers { get; set; } = new List<EntityUser>();
    public virtual ICollection<PropertyListing> Listings { get; set; } = new List<PropertyListing>();
    public virtual ICollection<Demand> Demands { get; set; } = new List<Demand>();
}
