using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Read-only viewer for the append-only audit trail (spec §19, Stage 12). AuditLogs
/// carries no RLS (it's not per-organization private data — it's an association-wide security
/// record, same reasoning as Notifications having none) so audit.read is the only gate; its
/// append-only-ness is enforced by a database trigger (Data/Security/AuditLogAppendOnly.sql),
/// tested in rls_test.sql since Stage 3, regardless of what this controller does.</summary>
[ApiController]
[Route("api/audit-logs")]
[Authorize]
[RequirePermission("audit.read")]
public class AuditLogsController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? entityType, [FromQuery] string? action, [FromQuery] Guid? actorProfileId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var q = db.AuditLogs.AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType)) q = q.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(action)) q = q.Where(a => a.Action == action);
        if (actorProfileId is not null) q = q.Where(a => a.ActorProfileId == actorProfileId);

        q = q.OrderByDescending(a => a.CreatedAt);

        pageSize = Math.Clamp(pageSize, 1, 200);
        page = Math.Max(page, 1);

        var totalCount = await q.CountAsync(ct);
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id, a.Action, a.EntityType, a.EntityId, a.Summary, a.IpAddress, a.CreatedAt,
                ActorName = a.ActorProfile != null ? a.ActorProfile.FullName : null,
            })
            .ToListAsync(ct);

        return Ok(new { items, totalCount, page, pageSize });
    }

    [HttpGet("entity-types")]
    public async Task<IActionResult> ListEntityTypes(CancellationToken ct) =>
        Ok(await db.AuditLogs.Select(a => a.EntityType).Distinct().OrderBy(t => t).ToListAsync(ct));
}
