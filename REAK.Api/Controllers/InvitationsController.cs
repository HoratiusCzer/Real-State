using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Auth;
using REAK.Api.Services.Security;

namespace REAK.Api.Controllers;

[ApiController]
[Route("api/invitations")]
public class InvitationsController(IInvitationService invitationService) : ControllerBase
{
    /// <summary>Admin-only — creates and emails an invitation (spec §2.4 Flow A step 3). Full
    /// invitation management (listing, filtering, resending) belongs to the Admin Portal, Stage 11;
    /// this is the minimal endpoint needed to actually exercise the flow end-to-end now.</summary>
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
    public async Task<IActionResult> Lookup(string token, CancellationToken ct)
    {
        var result = await invitationService.LookupAsync(token, ct);
        return result.Found ? Ok(result) : NotFound();
    }

    [HttpPost("{token}/accept")]
    [AllowAnonymous]
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
