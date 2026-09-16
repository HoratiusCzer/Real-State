using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.FeatureFlags;

/// <summary>The 13 flags from spec §15. Defaults must be conservative — seeded to false.</summary>
public class FeatureFlag
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(UpdatedByProfile))]
    public Guid? UpdatedByProfileId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile? UpdatedByProfile { get; set; }
}
