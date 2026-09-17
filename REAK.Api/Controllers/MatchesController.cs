using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Matching;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Collaboration;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Member-facing match browsing and actions (spec §12.1). None of the matching tables
/// (Match/MatchComponent/MatchRuleSet/MatchRule/MatchAction) have RLS — a Match row reveals a
/// compatibility relationship between two specific organizations' private data, which is at
/// least as sensitive as either side alone, so this controller does its own explicit ownership
/// check everywhere rather than leaning on RLS: a caller may only see/act on a match where their
/// org owns the listing OR the demand (or they're a system admin). Deliberately more
/// conservative than Listing/Demand RLS itself — network-shared visibility of one side does not
/// imply visibility of the match.
///
/// A subtlety this controller has to work around: every query here joins through
/// PropertyListings/Demands (to read MemberEntityId, title, reference code, etc.), and those two
/// tables DO have RLS. SQL Server applies a table's filter predicate to every reference to it —
/// including as a JOIN target — regardless of which columns are projected, so an ordinary query
/// like ".Where(m => callerOwnsListing || callerOwnsDemand)" still silently drops a row the
/// caller legitimately should see via the listing side, the moment the same query also joins to
/// the demand side and the caller can't see *that* row under their own RLS context (proven live:
/// a listing owner couldn't see their own match until this was fixed). The same
/// SessionContextOverride elevation MatchingEngine uses solves it here too — elevate just long
/// enough to run the query, then apply this controller's own explicit ownership check to decide
/// what the caller is actually allowed to see, exactly like MatchingEngine elevates only to
/// compute, never to decide who gets told about the result.</summary>
[ApiController]
[Route("api/matches")]
[Authorize]
[RequirePermission("matches.read")]
public class MatchesController(ReakDbContext db, SessionContextOverride sessionContextOverride, ICollaborationRequestService collaborationRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? listingId, [FromQuery] Guid? demandId, [FromQuery] MatchStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var caller = CallerContextFactory.From(User);

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            var matchingEnabled = await db.FeatureFlags.Where(f => f.Key == "matching_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
            var noRuleSetPublished = !await db.MatchRuleSets.AnyAsync(s => s.Status == MatchRuleSetStatus.Published, ct);
            if (!matchingEnabled || noRuleSetPublished)
            {
                return Ok(new { configured = false, result = new MatchSearchResult([], 0, page, pageSize) });
            }

            var q = db.Matches.AsQueryable();

            if (!caller.IsSystemAdmin)
            {
                q = q.Where(m => caller.MemberEntityIds.Contains(m.Listing.MemberEntityId) || caller.MemberEntityIds.Contains(m.Demand.MemberEntityId));
            }
            if (listingId is not null) q = q.Where(m => m.ListingId == listingId);
            if (demandId is not null) q = q.Where(m => m.DemandId == demandId);
            if (status is not null) q = q.Where(m => m.Status == status);

            // Default view only shows matches whose both sides are still live — a stale match
            // against an archived listing/fulfilled demand stays in the database for audit
            // history (spec §12: "auditable") but doesn't clutter the active browsing view.
            q = q.Where(m => m.Listing.Status == ListingStatus.Approved && !m.Listing.IsDeleted && m.Demand.Status == DemandStatus.Active && !m.Demand.IsDeleted);

            q = q.OrderByDescending(m => m.Score).ThenByDescending(m => m.ComputedAt);

            pageSize = Math.Clamp(pageSize, 1, 100);
            page = Math.Max(page, 1);

            var totalCount = await q.CountAsync(ct);
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MatchSummaryDto(
                    m.Id, m.ListingId, m.Listing.Title, m.Listing.ReferenceCode, m.Listing.MemberEntity.Name,
                    m.DemandId, m.Demand.Title, m.Demand.ReferenceCode, m.Demand.MemberEntity.Name,
                    m.Score, m.Status.ToString(), m.ComputedAt))
                .ToListAsync(ct);

            return Ok(new { configured = true, result = new MatchSearchResult(items, totalCount, page, pageSize) });
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            var match = await db.Matches
                .Include(m => m.Listing).ThenInclude(l => l.MemberEntity)
                .Include(m => m.Demand).ThenInclude(d => d.MemberEntity)
                .Include(m => m.MatchRuleSet)
                .Include(m => m.Components)
                .Include(m => m.Actions).ThenInclude(a => a.Profile)
                .FirstOrDefaultAsync(m => m.Id == id, ct);

            if (match is null) return NotFound();
            if (!CanAccess(caller, match)) return Forbid();

            return Ok(ToDetailDto(match));
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }

    [HttpPost("{id:guid}/actions")]
    public async Task<IActionResult> RecordAction(Guid id, RecordMatchActionRequest request, CancellationToken ct)
    {
        var caller = CallerContextFactory.From(User);

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            var match = await db.Matches
                .Include(m => m.Listing)
                .Include(m => m.Demand)
                .FirstOrDefaultAsync(m => m.Id == id, ct);
            if (match is null) return NotFound();
            if (!CanAccess(caller, match)) return Forbid();

            db.MatchActions.Add(new MatchAction
            {
                Id = Guid.NewGuid(),
                MatchId = id,
                ProfileId = caller.ProfileId,
                ActionType = request.ActionType,
                Notes = request.Notes,
            });

            match.Status = request.ActionType switch
            {
                MatchActionType.Shortlist => MatchStatus.Shortlisted,
                MatchActionType.Dismiss => MatchStatus.Dismissed,
                MatchActionType.Reopen => MatchStatus.Reopened,
                _ => match.Status,
            };

            await db.SaveChangesAsync(ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }

        // RequestCollaboration also files the actual CollaborationRequest (spec §13, Flow D step
        // 1) — kept outside the block above so CollaborationRequestService's own elevation isn't
        // nested inside this controller's, since both share the same scoped SessionContextOverride
        // instance and resetting it early would prematurely de-elevate the outer block.
        Guid? collaborationRequestId = null;
        if (request.ActionType == MatchActionType.RequestCollaboration)
        {
            var (op, requestId) = await collaborationRequestService.CreateFromMatchAsync(caller, id, request.Notes, ct);
            if (op.Result == CollabOpResult.Success) collaborationRequestId = requestId;
        }

        return Ok(new { recorded = true, collaborationRequestId });
    }

    private static bool CanAccess(CallerContext caller, Match match) =>
        caller.IsSystemAdmin || caller.MemberEntityIds.Contains(match.Listing.MemberEntityId) || caller.MemberEntityIds.Contains(match.Demand.MemberEntityId);

    private static MatchDetailDto ToDetailDto(Match m) => new(
        m.Id,
        m.ListingId, m.Listing.Title, m.Listing.ReferenceCode, m.Listing.MemberEntityId, m.Listing.MemberEntity.Name,
        m.DemandId, m.Demand.Title, m.Demand.ReferenceCode, m.Demand.MemberEntityId, m.Demand.MemberEntity.Name,
        m.Score, m.Status.ToString(),
        m.MatchRuleSetId, m.MatchRuleSet.Name, m.MatchRuleSet.Version,
        m.ComputedAt,
        m.Components.Select(c => new MatchComponentDto(c.Criterion, c.Result, c.NumericDelta, c.DetailText)).ToList(),
        m.Actions.OrderByDescending(a => a.CreatedAt).Select(a => new MatchActionDto(a.Id, a.ActionType, a.Notes, a.CreatedAt, a.Profile.FullName)).ToList());
}
