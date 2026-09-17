using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Services.Auth;

namespace REAK.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, ReakDbContext db) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Success || result.Tokens is null)
        {
            return Unauthorized(new { error = result.Error });
        }

        return ToResponse(result.Tokens);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Success || result.Tokens is null)
        {
            return Unauthorized(new { error = result.Error });
        }

        return ToResponse(result.Tokens);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await authService.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var profileId = GetProfileId();
        var profile = await db.Profiles
            .Where(p => p.Id == profileId)
            .Select(p => new { p.Id, p.Email, p.FullName, p.Phone, p.LastLoginAt })
            .FirstOrDefaultAsync(ct);

        if (profile is null)
        {
            return NotFound();
        }

        var memberships = await db.EntityUsers
            .Where(eu => eu.ProfileId == profileId && eu.IsActive)
            .Select(eu => new { eu.MemberEntityId, MemberEntityName = eu.MemberEntity.Name })
            .ToListAsync(ct);

        var permissions = User.Claims.Where(c => c.Type == Services.Security.ClaimsNames.Permission).Select(c => c.Value).Distinct();
        var isSystemAdmin = User.HasClaim(Services.Security.ClaimsNames.IsSystemAdmin, "true");

        return Ok(new { profile, memberships, permissions, isSystemAdmin });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var success = await authService.ChangePasswordAsync(GetProfileId(), request.CurrentPassword, request.NewPassword, ct);
        return success ? NoContent() : BadRequest(new { error = "Current password is incorrect, or the new password is too short." });
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await authService.ForgotPasswordAsync(request.Email, ct);
        // Always the same response whether or not the account exists.
        return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        var success = await authService.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return success ? NoContent() : BadRequest(new { error = "Reset token is invalid or expired." });
    }

    private Guid GetProfileId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    private static TokenResponse ToResponse(AuthTokens tokens) =>
        new(tokens.AccessToken, tokens.AccessTokenExpiresAt, tokens.RefreshToken, tokens.RefreshTokenExpiresAt);
}
