using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Services.Security;

public record AccessToken(string Token, DateTime ExpiresAt);

public interface IJwtTokenService
{
    AccessToken IssueAccessToken(Profile profile, UserClaims claims);

    /// <summary>Returns the raw token (given to the caller once, never stored) and the entity to
    /// persist (only its hash).</summary>
    (string RawToken, RefreshToken Entity) IssueRefreshToken(Guid profileId, string? createdByIp);

    string HashToken(string rawToken);
}
