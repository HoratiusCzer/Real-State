using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Matching;

/// <summary>A versioned, admin-authored set of matching rules (spec §12). Only a Published rule set
/// may generate production matches — if none is published, the engine must show "Matching is not yet
/// configured by REAK" rather than generating score-0 matches. No weights are hardcoded in application
/// code; they live entirely in MatchRule rows.</summary>
public class MatchRuleSet
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Version { get; set; }

    [Required]
    public MatchRuleSetStatus Status { get; set; } = MatchRuleSetStatus.Draft;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    [ForeignKey(nameof(CreatedByProfile))]
    public Guid CreatedByProfileId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public virtual Profile CreatedByProfile { get; set; } = null!;
    public virtual ICollection<MatchRule> Rules { get; set; } = new List<MatchRule>();
}
