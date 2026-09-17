using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Spec §4.2's Member Portal dashboard — "database-backed, no fake numbers". Every count
/// here is a real query scoped to the caller's own organization(s); it is legitimately all zeros
/// today because nothing can create a listing/demand/match/collaboration yet (Stages 6-9). Scoped
/// explicitly to the caller's own MemberEntityIds rather than relying on PropertyListings/Demands'
/// RLS filter alone — RLS's read predicate also allows network-shared listings from OTHER
/// organizations, which is a broader set than "my dashboard" should mean.
///
/// PotentialMatchesCount joins through both PropertyListings and Demands (RLS-protected tables)
/// to read MemberEntityId off each side — same issue MatchesController hit and fixed: an INNER
/// JOIN to an RLS-protected table silently drops a row the caller should see via the *other*
/// side alone, so this needs the same SessionContextOverride elevation for that one query, with
/// the explicit MemberEntityIds.Contains(...) check (already present) remaining the real
/// authorization.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(ReakDbContext db, SessionContextOverride sessionContextOverride) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummary>> Summary(CancellationToken ct)
    {
        var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var myEntityIds = await db.EntityUsers
            .Where(eu => eu.ProfileId == profileId && eu.IsActive)
            .Select(eu => eu.MemberEntityId)
            .ToListAsync(ct);

        var savedCount = await db.SavedListings.CountAsync(s => s.ProfileId == profileId, ct);

        if (myEntityIds.Count == 0)
        {
            return Ok(new DashboardSummary(0, 0, 0, 0, 0, savedCount, 0, []));
        }

        var now = DateTime.UtcNow;
        var expiryHorizon = now.AddDays(7);

        var activeListings = db.PropertyListings.Where(l =>
            myEntityIds.Contains(l.MemberEntityId) && !l.IsDeleted && l.Status == ListingStatus.Approved &&
            (l.ExpiresAt == null || l.ExpiresAt > now));

        var draftListings = db.PropertyListings.Where(l =>
            myEntityIds.Contains(l.MemberEntityId) && !l.IsDeleted && l.Status == ListingStatus.Draft);

        var activeDemands = db.Demands.Where(d =>
            myEntityIds.Contains(d.MemberEntityId) && !d.IsDeleted && d.Status == DemandStatus.Active);

        sessionContextOverride.IsSystemLevel = true;
        int potentialMatchesCount;
        try
        {
            potentialMatchesCount = await db.Matches.CountAsync(m =>
                m.Status == MatchStatus.New &&
                (myEntityIds.Contains(m.Listing.MemberEntityId) || myEntityIds.Contains(m.Demand.MemberEntityId)), ct);
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }

        var pendingCollaborationRequests = db.CollaborationRequests.Where(r =>
            myEntityIds.Contains(r.ToMemberEntityId) && r.Status == CollaborationRequestStatus.Pending);

        var expiringListings = db.PropertyListings.Where(l =>
            myEntityIds.Contains(l.MemberEntityId) && !l.IsDeleted &&
            l.ExpiresAt != null && l.ExpiresAt > now && l.ExpiresAt <= expiryHorizon);

        var expiringDemands = db.Demands.Where(d =>
            myEntityIds.Contains(d.MemberEntityId) && !d.IsDeleted &&
            d.ExpiresAt != null && d.ExpiresAt > now && d.ExpiresAt <= expiryHorizon);

        var recentProperties = await db.PropertyListings
            .Where(l => myEntityIds.Contains(l.MemberEntityId) && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new RecentPropertyDto(l.Id, l.ReferenceCode, l.Title, l.Status.ToString(), l.CreatedAt))
            .ToListAsync(ct);

        var summary = new DashboardSummary(
            ActivePropertiesCount: await activeListings.CountAsync(ct),
            DraftPropertiesCount: await draftListings.CountAsync(ct),
            ActiveRequirementsCount: await activeDemands.CountAsync(ct),
            PotentialMatchesCount: potentialMatchesCount,
            PendingCollaborationRequestsCount: await pendingCollaborationRequests.CountAsync(ct),
            SavedPropertiesCount: savedCount,
            ExpiringItemsCount: await expiringListings.CountAsync(ct) + await expiringDemands.CountAsync(ct),
            RecentProperties: recentProperties);

        return Ok(summary);
    }
}
