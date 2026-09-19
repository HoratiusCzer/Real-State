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
[Route("api/membership-applications")]
public class MembershipApplicationsController(IMembershipApplicationService applicationService, ReakDbContext db) : ControllerBase
{
    /// <summary>Flow A step 1 (spec §2.4) — public submission. Gated by the
    /// membership_application_enabled feature flag (seeded off by default, spec §15).</summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Submit(SubmitMembershipApplicationRequest request, CancellationToken ct)
    {
        var (success, error, application) = await applicationService.SubmitAsync(
            request.CompanyName, request.ContactName, request.Email, request.Phone, request.Message, ct);

        return success
            ? Ok(new { application!.Id, application.Status })
            : BadRequest(new { error });
    }

    /// <summary>Admin review queue. Full admin UI is Stage 11 (Admin Portal) — this is the API
    /// surface needed to exercise Flow A end-to-end now. Applications belong to no organization
    /// yet (that's the point — they're pre-approval), so unlike other members.read-gated
    /// endpoints there's no org to scope an ordinary MemberAdmin *to*; this is admin-only,
    /// full stop, same as ReportsController/AuditLogsController/FeatureFlagsController.</summary>
    [HttpGet]
    [Authorize]
    [RequirePermission("members.read")]
    public async Task<IActionResult> List([FromQuery] string? status, CancellationToken ct)
    {
        if (!User.HasClaim(ClaimsNames.IsSystemAdmin, "true")) return Forbid();

        var query = db.MembershipApplications.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Models.Enums.MembershipApplicationStatus>(status, true, out var parsed))
        {
            query = query.Where(a => a.Status == parsed);
        }

        var applications = await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
        return Ok(applications);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize]
    [RequirePermission("members.create")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var reviewedBy = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var (success, error) = await applicationService.ApproveAsync(id, reviewedBy, ct);
        return success ? NoContent() : BadRequest(new { error });
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize]
    [RequirePermission("members.create")]
    public async Task<IActionResult> Reject(Guid id, RejectMembershipApplicationRequest request, CancellationToken ct)
    {
        var reviewedBy = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var (success, error) = await applicationService.RejectAsync(id, reviewedBy, request.Reason, ct);
        return success ? NoContent() : BadRequest(new { error });
    }
}
