using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;

namespace REAK.Api.Controllers;

/// <summary>The public member directory (spec §4.1 `/members`, `/members/:slug`) — a route the
/// public site has linked to since Stage 2 but that never actually had a backend; closing it here
/// since it's exactly what `public_member_directory_enabled` (Stage 12) was seeded to gate.
/// Same sanitized-projection discipline as PublicPropertiesController/PublicContentController:
/// gated by the flag, and only ever exposes fields a member organization would expect to be public
/// (never Email/Phone/Address — those stay behind the authenticated /api/member-entities the
/// Member Portal directory already uses). MemberEntity has no Slug column (same as PropertyListing
/// — see properties/[slug]'s frontend comment); the id doubles as the route's :slug segment.</summary>
[ApiController]
[Route("api/public/members")]
[AllowAnonymous]
public class PublicMemberEntitiesController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var enabled = await db.FeatureFlags.Where(f => f.Key == "public_member_directory_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
        if (!enabled) return Ok(new { enabled = false, items = Array.Empty<object>() });

        var items = await db.MemberEntities
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name, m.Description, m.Website, m.LogoUrl })
            .ToListAsync(ct);

        return Ok(new { enabled = true, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var enabled = await db.FeatureFlags.Where(f => f.Key == "public_member_directory_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
        if (!enabled) return NotFound();

        var entity = await db.MemberEntities
            .Where(m => m.Id == id && m.IsActive)
            .Select(m => new { m.Id, m.Name, m.Description, m.Website, m.LogoUrl })
            .FirstOrDefaultAsync(ct);

        return entity is null ? NotFound() : Ok(entity);
    }
}
