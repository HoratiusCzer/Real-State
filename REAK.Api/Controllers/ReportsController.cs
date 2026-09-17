using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Spec §4.3 names "reports" as an Admin Portal screen without a dedicated spec section
/// describing its contents (unlike every other Stage 12 piece, which §15/§19 spell out exactly).
/// Kept deliberately modest rather than guessed-at: real, database-backed aggregate counts over
/// data that already exists, no invented metrics (spec §22) and no charting library this codebase
/// doesn't already depend on — plain numbers and breakdowns, same visual language as the Admin
/// Dashboard's stat tiles. Every listing/demand/match query here is association-wide, so it needs
/// the same SessionContextOverride elevation DashboardController.AdminSummary already uses.</summary>
[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(ReakDbContext db, SessionContextOverride sessionContextOverride) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        if (!User.HasClaim(ClaimsNames.IsSystemAdmin, "true")) return Forbid();

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            var listingsByStatus = await db.PropertyListings
                .Where(l => !l.IsDeleted)
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var demandsByStatus = await db.Demands
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var matchesByStatus = await db.Matches
                .GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var collaborationsByStatus = await db.CollaborationRequests
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToListAsync(ct);

            var since = DateTime.UtcNow.AddMonths(-11).Date;
            since = new DateTime(since.Year, since.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var memberGrowth = await db.MemberEntities
                .Where(m => m.CreatedAt >= since)
                .GroupBy(m => new { m.CreatedAt.Year, m.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync(ct);

            return Ok(new
            {
                listingsByStatus,
                demandsByStatus,
                matchesByStatus,
                collaborationsByStatus,
                memberGrowth = memberGrowth
                    .Select(m => new { Month = $"{m.Year:D4}-{m.Month:D2}", m.Count })
                    .OrderBy(m => m.Month),
            });
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }
}
