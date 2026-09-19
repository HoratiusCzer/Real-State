using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Auth;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

[ApiController]
[Route("api/invitations")]
public class InvitationsController(IInvitationService invitationService, ReakDbContext db) : ControllerBase
{
    /// <summary>Admin Portal invitation queue (Stage 11) — everyone who was ever invited, newest
    /// first, so an admin can see what's pending/accepted/expired/revoked without hunting through
    /// individual member org pages. A system admin gets that full queue; an ordinary org-scoped
    /// MemberAdmin (who also holds members.read, for the invite-role-picker) only sees invitations
    /// for the org(s) they administer — same "permission slug alone isn't enough" gap already
    /// guarded against in ProfilesController.List and MemberEntitiesController.Update.</summary>
    [HttpGet]
    [Authorize]
    [RequirePermission("members.read")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var isSystemAdmin = User.HasClaim(ClaimsNames.IsSystemAdmin, "true");
        var query = db.Invitations.AsQueryable();

        if (!isSystemAdmin)
        {
            var profileId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var myEntityIds = await db.ProfileRoleAssignments
                .Where(a => a.ProfileId == profileId && a.Role.Name == "MemberAdmin" && a.MemberEntityId != null)
                .Select(a => a.MemberEntityId!.Value)
                .ToListAsync(ct);
            query = query.Where(i => i.MemberEntityId != null && myEntityIds.Contains(i.MemberEntityId.Value));
        }

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id, i.Email, i.Status, i.MemberEntityId, MemberEntityName = i.MemberEntity != null ? i.MemberEntity.Name : null,
                RoleName = i.Role.Name, InvitedByName = i.InvitedByProfile.FullName, i.ExpiresAt, i.AcceptedAt, i.CreatedAt,
            })
            .ToListAsync(ct);
        return Ok(invitations);
    }

    /// <summary>Admin-only — creates and emails an invitation (spec §2.4 Flow A step 3).</summary>
    [HttpPost]
    [Authorize]
    [RequirePermission("members.create")]
    public async Task<IActionResult> Create(CreateInvitationRequest request, CancellationToken ct)
    {
        try
        {
            var invitedBy = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var invitation = await invitationService.CreateAsync(request.Email, request.MemberEntityId, request.RoleId, invitedBy, ct);
            return Ok(new { invitation.Id, invitation.Email, invitation.ExpiresAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Lookup(string token, CancellationToken ct)
    {
        var result = await invitationService.LookupAsync(token, ct);
        return result.Found ? Ok(result) : NotFound();
    }

    [HttpPost("{token}/accept")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Accept(string token, AcceptInvitationRequest request, CancellationToken ct)
    {
        var (success, error) = await invitationService.AcceptAsync(token, request.Password, ct);
        return success ? NoContent() : BadRequest(new { error });
    }

    [HttpPost("{id:guid}/revoke")]
    [Authorize]
    [RequirePermission("members.create")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct)
    {
        var success = await invitationService.RevokeAsync(id, ct);
        return success ? NoContent() : NotFound();
    }
}
