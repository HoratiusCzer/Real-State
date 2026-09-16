using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Services.Security;

/// <summary>Access tokens are short-lived, stateless JWTs signed with a symmetric key that must be
/// supplied via configuration (Jwt:Key, mapped from the REAK_JWT_KEY / Jwt__Key environment
/// variable — never committed, see Program.cs's startup guard). Refresh tokens are opaque random
/// values; only their SHA-256 hash is ever persisted, matching the same never-store-the-secret
/// principle applied to passwords.</summary>
public class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _key = configuration["REAK_JWT_KEY"]
        ?? throw new InvalidOperationException("REAK_JWT_KEY is not configured. Set it as an environment variable or a user-secret before starting the API.");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "REAK.Api";
    private readonly string _audience = configuration["Jwt:Audience"] ?? "REAK.Clients";
    private readonly int _accessTokenMinutes = int.TryParse(configuration["Jwt:AccessTokenMinutes"], out var m) ? m : 15;
    private readonly int _refreshTokenDays = int.TryParse(configuration["Jwt:RefreshTokenDays"], out var d) ? d : 30;

    public AccessToken IssueAccessToken(Profile profile, UserClaims claims)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_accessTokenMinutes);

        var claimsList = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, profile.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, profile.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimsNames.IsSystemAdmin, claims.IsSystemAdmin ? "true" : "false"),
        };
        claimsList.AddRange(claims.Permissions.Select(p => new Claim(ClaimsNames.Permission, p)));
        claimsList.AddRange(claims.MemberEntityIds.Select(id => new Claim(ClaimsNames.MemberEntityId, id.ToString())));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claimsList,
            expires: expiresAt,
            signingCredentials: credentials);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string RawToken, RefreshToken Entity) IssueRefreshToken(Guid profileId, string? createdByIp)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            TokenHash = HashToken(rawToken),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenDays),
        };
        return (rawToken, entity);
    }

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
