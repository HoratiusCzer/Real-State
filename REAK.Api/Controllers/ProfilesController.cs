using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfilesController(ReakDbContext db, IAuditLogService auditLogService) : ControllerBase
{
    /// <summary>Admin Portal "users" list (Stage 11, spec §4.3) — every profile, regardless of
    /// which org(s) it belongs to or none at all, so an admin can find and suspend anyone. Only a
    /// system admin gets that unscoped view, though: members.read is also granted to the ordinary
    /// org-scoped MemberAdmin role, and without this check that role could list every user across
    /// every organization, not just its own — the same "permission slug alone isn't enough" gap
    /// MemberEntitiesController.Update already guards against for org updates.</summary>
    [HttpGet]
    [RequirePermission("members.read")]
    public async Task<IActionResult> List([FromQuery] string? search, CancellationToken ct)
    {
        var isSystemAdmin = User.HasClaim(ClaimsNames.IsSystemAdmin, "true");
        var query = db.Profiles.AsQueryable();

        if (!isSystemAdmin)
        {
            var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var myEntityIds = await db.EntityUsers
                .Where(e => e.ProfileId == profileId && e.IsActive)
                .Select(e => e.MemberEntityId)
                .ToListAsync(ct);
            query = query.Where(p => p.EntityMemberships.Any(m => m.IsActive && myEntityIds.Contains(m.MemberEntityId)));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Email.Contains(search) || p.FullName.Contains(search));
        }

        var profiles = await query
            .OrderBy(p => p.FullName)
            .Select(p => new
            {
                p.Id, p.Email, p.FullName, p.Phone, p.IsActive, p.LastLoginAt, p.CreatedAt,
                Memberships = p.EntityMemberships.Where(m => m.IsActive).Select(m => m.MemberEntity.Name),
                Roles = p.RoleAssignments.Select(r => r.Role.Name),
            })
            .ToListAsync(ct);

        return Ok(profiles);
    }

    /// <summary>Self-service profile edit — no permission slug needed, every authenticated profile
    /// may edit its own name/phone. Email is deliberately not editable here (it's the login
    /// identifier and unique-indexed; changing it needs its own verification flow, out of scope for
    /// Stage 5).</summary>
    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken ct)
    {
        var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == profileId, ct);
        if (profile is null)
        {
            return NotFound();
        }

        profile.FullName = request.FullName;
        profile.Phone = request.Phone;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

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

        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(actorId, "ProfileSuspended", "Profile", id, $"{profile.Email} was suspended.", ct);

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

        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(actorId, "ProfileReactivated", "Profile", id, $"{profile.Email} was reactivated.", ct);

        return NoContent();
    }
}
