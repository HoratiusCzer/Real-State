using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Matching;

/// <summary>Audit trail of member actions on a match: shortlist, dismiss, reopen, request collaboration,
/// report incorrect data (spec §12.1).</summary>
public class MatchAction
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Match))]
    public Guid MatchId { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    public MatchActionType ActionType { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Match Match { get; set; } = null!;
    public virtual Profile Profile { get; set; } = null!;
}
