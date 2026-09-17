namespace REAK.Api.Services.Matching;

/// <summary>Flow C (spec §2.4, §12): whenever a listing or demand is created or updated, recompute
/// its matches against the opposite side. Entirely server-side, deterministic, and driven only by
/// the currently Published MatchRuleSet — if none exists, these are no-ops rather than generating
/// score-0 placeholder matches (spec §12: "do not generate score-0 matches").</summary>
public interface IMatchingEngine
{
    Task<int> RecomputeForListingAsync(Guid listingId, CancellationToken ct = default);
    Task<int> RecomputeForDemandAsync(Guid demandId, CancellationToken ct = default);
}
