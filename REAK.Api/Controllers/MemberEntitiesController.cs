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

/// <summary>The Member Portal's "browse other REAK members" directory (spec §4.2 /portal/members)
/// plus each org's own self-service profile editing (/portal/organization). MemberEntity has no
/// RLS yet (documented Stage 3/13 gap), so the ownership check on Update below is the only thing
/// standing between "I hold members.update somewhere" and "I can edit any organization" — it must
/// stay correct.</summary>
[ApiController]
[Route("api/member-entities")]
[Authorize]
public class MemberEntitiesController(ReakDbContext db, IAuditLogService auditLogService) : ControllerBase
{
    /// <summary>includeInactive is silently ignored for anyone but a system admin — the ordinary
    /// member directory (spec §4.2) must never leak a suspended org's existence to other members;
    /// only the Admin Portal's own members screen (Stage 11) needs to see it in order to reactivate
    /// it.</summary>
    [HttpGet]
    [RequirePermission("members.read")]
    public async Task<IActionResult> List([FromQuery] bool includeInactive, CancellationToken ct)
    {
        var showInactive = includeInactive && User.HasClaim(ClaimsNames.IsSystemAdmin, "true");
        var entities = await db.MemberEntities
            .Where(m => showInactive || m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name, m.Description, m.Website, m.LogoUrl, m.IsActive, m.CreatedAt })
            .ToListAsync(ct);

        return Ok(entities);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("members.read")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var entity = await db.MemberEntities
            .Where(m => m.Id == id && m.IsActive)
            .Select(m => new
            {
                m.Id, m.Name, m.Description, m.Website, m.Phone, m.Email, m.Address, m.LogoUrl, m.CreatedAt,
            })
            .FirstOrDefaultAsync(ct);

        return entity is null ? NotFound() : Ok(entity);
    }

    /// <summary>A MemberAdmin may update only the organization(s) they administer; a SuperAdmin
    /// may update any. Holding the members.update permission slug alone is not enough — that
    /// slug is shared with the Admin Portal's own member-management screens (Stage 11), which do
    /// need to reach every org.</summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("members.update")]
    public async Task<IActionResult> Update(Guid id, UpdateMemberEntityRequest request, CancellationToken ct)
    {
        var isSystemAdmin = User.HasClaim(ClaimsNames.IsSystemAdmin, "true");
        if (!isSystemAdmin)
        {
            var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var administersThisEntity = await db.ProfileRoleAssignments
                .AnyAsync(a => a.ProfileId == profileId && a.MemberEntityId == id && a.Role.Name == "MemberAdmin", ct);
            if (!administersThisEntity)
            {
                return Forbid();
            }
        }

        var entity = await db.MemberEntities.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.Website = request.Website;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.Address = request.Address;
        await db.SaveChangesAsync(ct);

        var updaterId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(updaterId, "MemberEntityUpdated", "MemberEntity", id, $"{entity.Name} was updated.", ct);

        return NoContent();
    }

    /// <summary>Admin-only (system-level) — unlike Update, a MemberAdmin must never be able to
    /// suspend their own organization. Suspending an org is a blunter instrument than suspending
    /// individual profiles (ProfilesController): it hides the org from the directory but
    /// deliberately does not touch its members' own IsActive — the two are separate levers.</summary>
    [HttpPost("{id:guid}/suspend")]
    [RequirePermission("members.suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        if (!User.HasClaim(ClaimsNames.IsSystemAdmin, "true")) return Forbid();

        var entity = await db.MemberEntities.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.IsActive = false;
        await db.SaveChangesAsync(ct);

        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(actorId, "MemberEntitySuspended", "MemberEntity", id, $"{entity.Name} was suspended.", ct);

        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [RequirePermission("members.suspend")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        if (!User.HasClaim(ClaimsNames.IsSystemAdmin, "true")) return Forbid();

        var entity = await db.MemberEntities.FirstOrDefaultAsync(m => m.Id == id, ct);
        if (entity is null) return NotFound();

        entity.IsActive = true;
        await db.SaveChangesAsync(ct);

        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(actorId, "MemberEntityReactivated", "MemberEntity", id, $"{entity.Name} was reactivated.", ct);

        return NoContent();
    }

    /// <summary>What member_export_enabled (spec §15, Stage 12) actually gates — a plain CSV of
    /// every member organization, admin-only. Simple string-concatenation CSV rather than a
    /// library dependency: every field here is admin-authored plain text (org name/description/
    /// contact details), never free-form user content, so the usual CSV-injection concerns don't
    /// apply, but commas/quotes/newlines are still escaped defensively.</summary>
    [HttpGet("export")]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var enabled = await db.FeatureFlags.Where(f => f.Key == "member_export_enabled").Select(f => f.IsEnabled).FirstOrDefaultAsync(ct);
        if (!enabled) return BadRequest(new { error = "Member export is not currently enabled." });

        var entities = await db.MemberEntities
            .OrderBy(m => m.Name)
            .Select(m => new { m.Name, m.Description, m.Website, m.Phone, m.Email, m.Address, m.IsActive, m.CreatedAt })
            .ToListAsync(ct);

        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Name,Description,Website,Phone,Email,Address,Active,CreatedAt");
        foreach (var e in entities)
        {
            csv.AppendLine(string.Join(",", CsvField(e.Name), CsvField(e.Description), CsvField(e.Website), CsvField(e.Phone), CsvField(e.Email), CsvField(e.Address), e.IsActive, e.CreatedAt.ToString("u")));
        }

        var actorId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        await auditLogService.LogAsync(actorId, "MemberEntitiesExported", "MemberEntity", null, $"{entities.Count} member organizations exported to CSV.", ct);

        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "member-organizations.csv");
    }

    private static string CsvField(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}
