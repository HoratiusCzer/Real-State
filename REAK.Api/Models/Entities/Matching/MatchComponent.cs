using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Matching;

/// <summary>Per-criterion breakdown of a Match, for the match explanation UI (spec §12.1): pass/fail/
/// partial, with the numeric delta and a human-readable detail.</summary>
public class MatchComponent
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Match))]
    public Guid MatchId { get; set; }

    [Required]
    public MatchCriterion Criterion { get; set; }

    [Required]
    public MatchComponentResult Result { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? NumericDelta { get; set; }

    [MaxLength(500)]
    public string? DetailText { get; set; }

    public virtual Match Match { get; set; } = null!;
}
