using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Audit;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Auth;

/// <summary>Real login/logout/refresh/password-reset (spec §6) — no fake login, no hardcoded
/// credentials, no frontend-only auth. Every check here (IsActive, expiry, revocation) is
/// re-verified against the database on the path that matters, not just trusted from a token.</summary>
public class AuthService(
    ReakDbContext db,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService,
    IUserClaimsFactory claimsFactory,
    IEmailSender emailSender,
    INotificationService notificationService,
    IAuditLogService auditLogService) : IAuthService
{
    private const int MinPasswordLength = 8;

    public async Task<AuthResult> LoginAsync(string email, string password, string? ip, CancellationToken ct = default)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Email == email, ct);

        // Deliberately generic error for both "no such account" and "wrong password" — distinguishing
        // them lets an attacker enumerate registered emails.
        if (profile is null || !profile.IsActive || !passwordHasher.Verify(password, profile.PasswordHash))
        {
            // Security event (spec §33: "Log: authentication failures... security events") — the
            // attempted email is logged for brute-force/enumeration detection, the password never
            // is. actorProfileId is null when the account doesn't exist at all; still logged, since
            // repeated failures against a *nonexistent* email are exactly what account enumeration
            // looks like.
            await auditLogService.LogAsync(profile?.Id, "LoginFailed", "Profile", profile?.Id, $"Failed login attempt for {email}.", ct);
            return new AuthResult(false, "Invalid email or password, or the account is inactive.", null);
        }

        profile.LastLoginAt = DateTime.UtcNow;
        var tokens = await IssueTokenPairAsync(profile, ip, ct);
        await db.SaveChangesAsync(ct);

        await auditLogService.LogAsync(profile.Id, "Login", "Profile", profile.Id, $"{profile.Email} logged in.", ct);

        return new AuthResult(true, null, tokens);
    }

    public async Task<AuthResult> RefreshAsync(string rawRefreshToken, string? ip, CancellationToken ct = default)
    {
        var hash = tokenService.HashToken(rawRefreshToken);
        var existing = await db.RefreshTokens
            .Include(r => r.Profile)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

        if (existing is null || existing.RevokedAt is not null || existing.ExpiresAt <= DateTime.UtcNow || !existing.Profile.IsActive)
        {
            return new AuthResult(false, "Refresh token is invalid, expired, or revoked.", null);
        }

        var tokens = await IssueTokenPairAsync(existing.Profile, ip, ct, revokeAndReplace: existing);
        await db.SaveChangesAsync(ct);

        return new AuthResult(true, null, tokens);
    }

    public async Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default)
    {
        var hash = tokenService.HashToken(rawRefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
        if (existing is not null && existing.RevokedAt is null)
        {
            existing.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ChangePasswordAsync(Guid profileId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        if (newPassword.Length < MinPasswordLength)
        {
            return false;
        }

        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == profileId, ct);
        if (profile is null || !passwordHasher.Verify(currentPassword, profile.PasswordHash))
        {
            return false;
        }

        profile.PasswordHash = passwordHasher.Hash(newPassword);
        await RevokeAllRefreshTokensAsync(profileId, ct);
        await db.SaveChangesAsync(ct);

        await notificationService.NotifyAsync(profileId, NotificationType.AccountEvent, "Your password was changed", null, "/portal/settings", ct);
        await auditLogService.LogAsync(profileId, "PasswordChanged", "Profile", profileId, $"{profile.Email} changed their password.", ct);

        return true;
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Email == email && p.IsActive, ct);
        // Always behave the same way whether or not the account exists — the caller can't tell.
        if (profile is null)
        {
            return;
        }

        var rawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            TokenHash = tokenService.HashToken(rawToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        });
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            profile.Email,
            "Reset your REAK password",
            $"A password reset was requested for your REAK account. Reset token: {rawToken} (expires in 1 hour). If you did not request this, ignore this email.",
            ct);
    }

    public async Task<bool> ResetPasswordAsync(string rawToken, string newPassword, CancellationToken ct = default)
    {
        if (newPassword.Length < MinPasswordLength)
        {
            return false;
        }

        var hash = tokenService.HashToken(rawToken);
        var resetToken = await db.PasswordResetTokens
            .Include(t => t.Profile)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (resetToken is null || resetToken.UsedAt is not null || resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        resetToken.Profile.PasswordHash = passwordHasher.Hash(newPassword);
        resetToken.UsedAt = DateTime.UtcNow;
        await RevokeAllRefreshTokensAsync(resetToken.ProfileId, ct);
        await db.SaveChangesAsync(ct);

        await notificationService.NotifyAsync(resetToken.ProfileId, NotificationType.AccountEvent, "Your password was reset", null, "/portal/settings", ct);
        await auditLogService.LogAsync(resetToken.ProfileId, "PasswordReset", "Profile", resetToken.ProfileId, $"{resetToken.Profile.Email} reset their password via a reset token.", ct);

        return true;
    }

    private async Task<AuthTokens> IssueTokenPairAsync(Profile profile, string? ip, CancellationToken ct, RefreshToken? revokeAndReplace = null)
    {
        var claims = await claimsFactory.BuildAsync(profile.Id, ct);
        var accessToken = tokenService.IssueAccessToken(profile, claims);
        var (rawRefresh, refreshEntity) = tokenService.IssueRefreshToken(profile.Id, ip);

        db.RefreshTokens.Add(refreshEntity);

        if (revokeAndReplace is not null)
        {
            revokeAndReplace.RevokedAt = DateTime.UtcNow;
            revokeAndReplace.ReplacedByTokenId = refreshEntity.Id;
        }

        return new AuthTokens(accessToken.Token, accessToken.ExpiresAt, rawRefresh, refreshEntity.ExpiresAt);
    }

    private async Task RevokeAllRefreshTokensAsync(Guid profileId, CancellationToken ct)
    {
        var tokens = await db.RefreshTokens
            .Where(r => r.ProfileId == profileId && r.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevokedAt = DateTime.UtcNow;
        }
    }
}
