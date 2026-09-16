using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Matching;

/// <summary>One scoring criterion within a MatchRuleSet. Weight and tolerance are admin-configured
/// data — never a literal in application code (spec §12, §24). Weight defaults to 0 (no effect) so an
/// admin must deliberately assign it; nothing here is a guessed "reasonable" figure.</summary>
public class MatchRule
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(MatchRuleSet))]
    public Guid MatchRuleSetId { get; set; }

    [Required]
    public MatchCriterion Criterion { get; set; }

    [Required]
    [Column(TypeName = "decimal(6,4)")]
    public decimal Weight { get; set; }

    /// <summary>If true, failing this criterion excludes the pair from matching entirely rather than
    /// just lowering the score.</summary>
    public bool IsRequired { get; set; }

    /// <summary>Criterion-specific tolerance (e.g. price tolerance %, area tolerance %) — meaning
    /// depends on Criterion; admin-configured, not hardcoded.</summary>
    [Column(TypeName = "decimal(10,4)")]
    public decimal? ToleranceValue { get; set; }

    public int SortOrder { get; set; }

    public virtual MatchRuleSet MatchRuleSet { get; set; } = null!;
}
