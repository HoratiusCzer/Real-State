using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.FeatureFlags;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

/// <summary>Admin management of the 13 flags from spec §15 (Stage 12). Reads are anonymous where a
/// flag gates public-facing behavior a page needs to check before rendering (mirrors
/// PublicPropertiesController/MembershipApplicationService's existing per-flag reads); this
/// controller's own list/toggle are admin-only (settings.manage). Toggling is itself one of spec
/// §19's explicit audit categories ("feature flag changes").</summary>
[ApiController]
[Route("api/feature-flags")]
[Authorize]
[RequirePermission("settings.manage")]
public class FeatureFlagsController(ReakDbContext db, IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct) =>
        Ok(await db.FeatureFlags.OrderBy(f => f.Key).Select(f => new FeatureFlagDto(f.Id, f.Key, f.IsEnabled, f.UpdatedAt)).ToListAsync(ct));

    [HttpPut("{key}")]
    public async Task<IActionResult> SetEnabled(string key, SetFeatureFlagRequest request, CancellationToken ct)
    {
        var flag = await db.FeatureFlags.FirstOrDefaultAsync(f => f.Key == key, ct);
        if (flag is null) return NotFound();
        if (flag.IsEnabled == request.IsEnabled) return NoContent();

        flag.IsEnabled = request.IsEnabled;
        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        flag.UpdatedByProfileId = actorId;
        flag.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditLogService.LogAsync(actorId, "FeatureFlagChanged", "FeatureFlag", flag.Id, $"{key} set to {(request.IsEnabled ? "enabled" : "disabled")}.", ct);

        return NoContent();
    }
}
