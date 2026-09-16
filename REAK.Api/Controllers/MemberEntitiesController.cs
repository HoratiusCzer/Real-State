using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
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
public class MemberEntitiesController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    [RequirePermission("members.read")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var entities = await db.MemberEntities
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .Select(m => new { m.Id, m.Name, m.Description, m.Website, m.LogoUrl, m.CreatedAt })
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

    /// <summary>A MemberAdmin may update only the organization(s) they administer; a SuperAdmin/
    /// AssociationAdmin may update any. Holding the members.update permission slug alone is not
    /// enough — that slug is shared with the Admin Portal's own member-management screens
    /// (Stage 11), which do need to reach every org.</summary>
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
        return NoContent();
    }
}
