using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Entities.Demands;
using REAK.Api.Models.Entities.Listings;
using REAK.Api.Models.Entities.Matching;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Matching;

/// <summary>The scoring core (spec §12). Every number that shapes a score — which criteria count,
/// how much each is worth, whether failing one excludes the pair, what tolerance is acceptable —
/// comes from the currently Published MatchRuleSet's rows, never a literal in this file. A
/// criterion with no data on either side (e.g. a requirement that never set a budget) is excluded
/// from both the numerator and denominator of the score rather than being scored as a fail — a
/// listing shouldn't be penalized for a preference the requirement simply never stated.
///
/// This is a spec §20 "security-sensitive database function" in disguise: it shares its
/// DbContext/connection with the HTTP request that triggered it, which means Stage 4's RLS
/// interceptor has already stamped that connection with the *calling user's* session context.
/// Left alone, that would make "find every Approved listing to match against this demand"
/// silently RLS-filter down to only what the caller can already see — exactly wrong for a
/// system-level computation that must see across every organization to do its job at all (the
/// caller only ever learns about a match through MatchesController's own explicit ownership
/// check, never by reading these queries' raw results). SessionContextOverride fixes that the
/// same way a Postgres SECURITY DEFINER function would: explicitly, narrowly, only for this
/// operation — set true at the start of a recompute and reset in a `finally`, so a request that
/// does something else after recomputing doesn't stay elevated by accident.</summary>
public class MatchingEngine(ReakDbContext db, SessionContextOverride sessionContextOverride, INotificationService notificationService) : IMatchingEngine
{
    public async Task<int> RecomputeForListingAsync(Guid listingId, CancellationToken ct = default)
    {
        var ruleSet = await GetPublishedRuleSetAsync(ct);
        if (ruleSet is null) return 0;

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            return await RecomputeForListingCoreAsync(listingId, ruleSet, ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }

    public async Task<int> RecomputeForDemandAsync(Guid demandId, CancellationToken ct = default)
    {
        var ruleSet = await GetPublishedRuleSetAsync(ct);
        if (ruleSet is null) return 0;

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            return await RecomputeForDemandCoreAsync(demandId, ruleSet, ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }

    private async Task<int> RecomputeForListingCoreAsync(Guid listingId, MatchRuleSet ruleSet, CancellationToken ct)
    {
        var listing = await db.PropertyListings
            .Include(l => l.ListingAmenities)
            .FirstOrDefaultAsync(l => l.Id == listingId && !l.IsDeleted && l.Status == ListingStatus.Approved, ct);
        if (listing is null) return 0;

        var demands = await db.Demands
            .Include(d => d.PropertyTypes)
            .Include(d => d.Locations)
            .Include(d => d.DemandAmenities)
            .Where(d => !d.IsDeleted && d.Status == DemandStatus.Active)
            .ToListAsync(ct);

        var count = 0;
        foreach (var demand in demands)
        {
            if (await UpsertMatchAsync(listing, demand, ruleSet, ct)) count++;
        }
        return count;
    }

    private async Task<int> RecomputeForDemandCoreAsync(Guid demandId, MatchRuleSet ruleSet, CancellationToken ct)
    {
        var demand = await db.Demands
            .Include(d => d.PropertyTypes)
            .Include(d => d.Locations)
            .Include(d => d.DemandAmenities)
            .FirstOrDefaultAsync(d => d.Id == demandId && !d.IsDeleted && d.Status == DemandStatus.Active, ct);
        if (demand is null) return 0;

        var listings = await db.PropertyListings
            .Include(l => l.ListingAmenities)
            .Where(l => !l.IsDeleted && l.Status == ListingStatus.Approved)
            .ToListAsync(ct);

        var count = 0;
        foreach (var listing in listings)
        {
            if (await UpsertMatchAsync(listing, demand, ruleSet, ct)) count++;
        }
        return count;
    }

    private async Task<MatchRuleSet?> GetPublishedRuleSetAsync(CancellationToken ct)
    {
        // matching_enabled (spec §15, Stage 12) is a belt-and-suspenders kill switch on top of the
        // existing "is a rule set published" gate — an admin can pause matching for maintenance
        // without un-publishing (and thereby losing) the rule set itself.
        var enabled = await db.FeatureFlags.Where(f => f.Key == "matching_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
        if (!enabled) return null;

        return await db.MatchRuleSets.Include(s => s.Rules).FirstOrDefaultAsync(s => s.Status == MatchRuleSetStatus.Published, ct);
    }

    private async Task<bool> UpsertMatchAsync(PropertyListing listing, Demand demand, MatchRuleSet ruleSet, CancellationToken ct)
    {
        var components = new List<(MatchCriterion Criterion, MatchComponentResult Result, decimal? Delta, string Detail)>();
        var excluded = false;

        foreach (var rule in ruleSet.Rules)
        {
            var (result, delta, detail) = Evaluate(rule, listing, demand);
            components.Add((rule.Criterion, result, delta, detail));
            if (rule.IsRequired && result == MatchComponentResult.Fail)
            {
                excluded = true;
            }
        }

        var existing = await db.Matches
            .Include(m => m.Components)
            .FirstOrDefaultAsync(m => m.ListingId == listing.Id && m.DemandId == demand.Id, ct);

        if (excluded)
        {
            // A previously-qualifying pair may stop qualifying after an edit (e.g. price moved
            // outside the required range) — remove any stale Match rather than leaving it.
            if (existing is not null)
            {
                db.MatchComponents.RemoveRange(existing.Components);
                db.Matches.Remove(existing);
                await db.SaveChangesAsync(ct);
            }
            return false;
        }

        var score = ComputeScore(ruleSet.Rules, components);

        var isNewMatch = existing is null;
        if (existing is null)
        {
            existing = new Match
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                DemandId = demand.Id,
                MatchRuleSetId = ruleSet.Id,
                Score = score,
                Status = MatchStatus.New,
                ComputedAt = DateTime.UtcNow,
            };
            db.Matches.Add(existing);
        }
        else
        {
            existing.MatchRuleSetId = ruleSet.Id;
            existing.Score = score;
            existing.ComputedAt = DateTime.UtcNow;
            // Status (New/Shortlisted/Dismissed/Reopened) is member-driven and deliberately left
            // untouched by recomputation — an edit shouldn't silently un-dismiss a match a member
            // already reviewed and decided against.
            db.MatchComponents.RemoveRange(existing.Components);
        }

        foreach (var (criterion, result, delta, detail) in components)
        {
            db.MatchComponents.Add(new MatchComponent
            {
                Id = Guid.NewGuid(),
                MatchId = existing.Id,
                Criterion = criterion,
                Result = result,
                NumericDelta = delta,
                DetailText = detail,
            });
        }

        await db.SaveChangesAsync(ct);

        // Only a genuinely new pair is worth notifying about — a recompute that just refreshes an
        // already-known match's score would otherwise notify both orgs on every listing/demand edit.
        if (isNewMatch)
        {
            var linkUrl = $"/portal/matches/{existing.Id}";
            await notificationService.NotifyOrgAsync(listing.MemberEntityId, NotificationType.Match, $"New match for \"{listing.Title}\"", $"Scored {score:0}% against a requirement.", linkUrl, ct);
            await notificationService.NotifyOrgAsync(demand.MemberEntityId, NotificationType.Match, $"New match for \"{demand.Title}\"", $"Scored {score:0}% against a listing.", linkUrl, ct);
        }

        return true;
    }

    private static decimal ComputeScore(
        ICollection<MatchRule> rules,
        List<(MatchCriterion Criterion, MatchComponentResult Result, decimal? Delta, string Detail)> components)
    {
        decimal weightedSum = 0;
        decimal totalWeight = 0;

        foreach (var rule in rules)
        {
            var component = components.First(c => c.Criterion == rule.Criterion);
            if (component.Result == MatchComponentResult.MissingData)
            {
                continue;
            }

            var contribution = component.Result switch
            {
                MatchComponentResult.Pass => 1m,
                MatchComponentResult.Partial => 0.5m,
                _ => 0m,
            };
            weightedSum += rule.Weight * contribution;
            totalWeight += rule.Weight;
        }

        return totalWeight == 0 ? 0 : Math.Round(weightedSum / totalWeight * 100m, 2);
    }

    private static (MatchComponentResult Result, decimal? Delta, string Detail) Evaluate(MatchRule rule, PropertyListing listing, Demand demand) =>
        rule.Criterion switch
        {
            MatchCriterion.Location => EvaluateLocation(listing, demand),
            MatchCriterion.Price => EvaluateRange(listing.Price, demand.MinBudget, demand.MaxBudget, rule.ToleranceValue, "budget"),
            MatchCriterion.Area => EvaluateRange(listing.LandArea, demand.MinArea, demand.MaxArea, rule.ToleranceValue, "area"),
            MatchCriterion.PropertyType => EvaluatePropertyType(listing, demand),
            MatchCriterion.Purpose => EvaluatePurpose(listing, demand),
            MatchCriterion.Bedrooms => EvaluateMinimum(listing.Bedrooms, demand.MinBedrooms, "bedroom"),
            MatchCriterion.Bathrooms => EvaluateMinimum(listing.Bathrooms, demand.MinBathrooms, "bathroom"),
            // Demand has no furnishing-preference field in the schema (spec §9 never asked for
            // one) — always MissingData, honestly, rather than guessing a preference.
            MatchCriterion.Furnishing => (MatchComponentResult.MissingData, null, "Requirements don't currently capture a furnishing preference."),
            MatchCriterion.Amenities => EvaluateAmenities(listing, demand),
            _ => (MatchComponentResult.MissingData, null, "Unrecognized criterion."),
        };

    private static (MatchComponentResult, decimal?, string) EvaluateLocation(PropertyListing listing, Demand demand)
    {
        if (demand.Locations.Count == 0)
        {
            return (MatchComponentResult.Pass, null, "Requirement did not restrict location.");
        }

        foreach (var loc in demand.Locations)
        {
            if (loc.ProvinceId != listing.ProvinceId) continue;
            if (loc.DistrictId is { } d && d != listing.DistrictId) continue;
            if (loc.MunicipalityId is { } m && m != listing.MunicipalityId) continue;
            if (loc.WardId is { } w && w != listing.WardId) continue;
            if (loc.LocalityId is { } lo && lo != listing.LocalityId) continue;
            return (MatchComponentResult.Pass, null, "Listing location matches one of the requirement's acceptable locations.");
        }

        return (MatchComponentResult.Fail, null, "Listing location does not match any of the requirement's acceptable locations.");
    }

    private static (MatchComponentResult, decimal?, string) EvaluateRange(decimal value, decimal? min, decimal? max, decimal? tolerancePercent, string label)
    {
        if (min is null && max is null)
        {
            return (MatchComponentResult.MissingData, null, $"Requirement did not specify a {label} range.");
        }

        if ((min is null || value >= min) && (max is null || value <= max))
        {
            return (MatchComponentResult.Pass, 0m, $"Within the requirement's {label} range.");
        }

        decimal distance;
        decimal nearestBound;
        if (max is not null && value > max)
        {
            distance = value - max.Value;
            nearestBound = max.Value;
        }
        else
        {
            distance = min!.Value - value;
            nearestBound = min.Value;
        }

        var percentOver = nearestBound > 0 ? distance / nearestBound * 100m : 100m;
        percentOver = Math.Round(percentOver, 2);

        if (tolerancePercent is { } tol && percentOver <= tol)
        {
            return (MatchComponentResult.Partial, percentOver, $"{percentOver:0.##}% outside the requirement's {label} range (within the configured tolerance).");
        }

        return (MatchComponentResult.Fail, percentOver, $"{percentOver:0.##}% outside the requirement's {label} range.");
    }

    private static (MatchComponentResult, decimal?, string) EvaluatePropertyType(PropertyListing listing, Demand demand)
    {
        if (demand.PropertyTypes.Count == 0)
        {
            return (MatchComponentResult.Pass, null, "Requirement did not restrict property type.");
        }

        return demand.PropertyTypes.Any(pt => pt.PropertyTypeId == listing.PropertyTypeId)
            ? (MatchComponentResult.Pass, null, "Listing's property type is among the requirement's accepted types.")
            : (MatchComponentResult.Fail, null, "Listing's property type is not among the requirement's accepted types.");
    }

    private static (MatchComponentResult, decimal?, string) EvaluatePurpose(PropertyListing listing, Demand demand) =>
        listing.PurposeId == demand.PurposeId
            ? (MatchComponentResult.Pass, null, "Purposes match.")
            : (MatchComponentResult.Fail, null, "Purposes differ.");

    private static (MatchComponentResult, decimal?, string) EvaluateMinimum(int? listingValue, int? demandMinimum, string label)
    {
        if (demandMinimum is null)
        {
            return (MatchComponentResult.MissingData, null, $"Requirement did not specify a minimum {label} count.");
        }
        if (listingValue is null)
        {
            return (MatchComponentResult.MissingData, null, $"Listing did not specify a {label} count.");
        }

        return listingValue >= demandMinimum
            ? (MatchComponentResult.Pass, null, $"Listing meets the requirement's minimum {label} count.")
            : (MatchComponentResult.Fail, (decimal)(demandMinimum - listingValue), $"Listing has fewer {label}s than required.");
    }

    private static (MatchComponentResult, decimal?, string) EvaluateAmenities(PropertyListing listing, Demand demand)
    {
        if (demand.DemandAmenities.Count == 0)
        {
            return (MatchComponentResult.Pass, null, "Requirement did not specify desired amenities.");
        }

        var desired = demand.DemandAmenities.Select(a => a.AmenityId).ToHashSet();
        var listingAmenities = listing.ListingAmenities.Select(a => a.AmenityId).ToHashSet();
        var overlap = desired.Intersect(listingAmenities).Count();
        var ratio = (decimal)overlap / desired.Count;

        if (ratio == 1m) return (MatchComponentResult.Pass, ratio * 100m, "Listing has all of the requirement's desired amenities.");
        if (ratio > 0m) return (MatchComponentResult.Partial, ratio * 100m, $"Listing has {overlap} of {desired.Count} desired amenities.");
        return (MatchComponentResult.Fail, 0m, "Listing has none of the requirement's desired amenities.");
    }
}
