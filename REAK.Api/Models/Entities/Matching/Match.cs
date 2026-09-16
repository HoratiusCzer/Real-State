using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Demands;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Matching;

/// <summary>A computed pairing between a listing and a demand (bidirectional — spec §12), produced by
/// evaluating the published MatchRuleSet at ComputedAt. Recomputing updates Score/RuleSet/ComputedAt on
/// the same row rather than inserting a duplicate — ListingId+DemandId is unique.</summary>
public class Match
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [Required]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [Required]
    [ForeignKey(nameof(MatchRuleSet))]
    public Guid MatchRuleSetId { get; set; }

    [Required]
    [Column(TypeName = "decimal(6,2)")]
    public decimal Score { get; set; }

    [Required]
    public MatchStatus Status { get; set; } = MatchStatus.New;

    public DateTime ComputedAt { get; set; } = DateTime.UtcNow;

    public virtual PropertyListing Listing { get; set; } = null!;
    public virtual Demand Demand { get; set; } = null!;
    public virtual MatchRuleSet MatchRuleSet { get; set; } = null!;
    public virtual ICollection<MatchComponent> Components { get; set; } = new List<MatchComponent>();
    public virtual ICollection<MatchAction> Actions { get; set; } = new List<MatchAction>();
}
