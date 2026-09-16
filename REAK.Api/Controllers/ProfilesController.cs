using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfilesController(ReakDbContext db) : ControllerBase
{
    /// <summary>Suspension must revoke access immediately, even for an already-active session (spec
    /// §2.2) — enforced by ActiveProfileMiddleware re-checking Profile.IsActive on every
    /// authenticated request, not just at login.</summary>
    [HttpPost("{id:guid}/suspend")]
    [RequirePermission("members.suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null)
        {
            return NotFound();
        }

        profile.IsActive = false;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [RequirePermission("members.suspend")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null)
        {
            return NotFound();
        }

        profile.IsActive = true;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
