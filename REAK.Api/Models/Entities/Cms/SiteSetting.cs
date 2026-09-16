using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Cms;

/// <summary>Generic key-value site configuration (Admin Portal "settings" route), distinct from
/// FeatureFlag — e.g. default OG image, social links. Never seeded with REAK's real contact info,
/// legal name, or colors — those are configuration the association itself must supply (spec §36).</summary>
public class SiteSetting
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Key { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Value { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
