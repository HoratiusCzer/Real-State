using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using REAK.Api.Services.Listings;

namespace REAK.Api.Services.Security;

public static class CallerContextFactory
{
    public static CallerContext From(ClaimsPrincipal user)
    {
        var profileId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var isSystemAdmin = user.HasClaim(ClaimsNames.IsSystemAdmin, "true");
        var memberEntityIds = user.Claims
            .Where(c => c.Type == ClaimsNames.MemberEntityId)
            .Select(c => Guid.Parse(c.Value))
            .ToList();

        return new CallerContext(profileId, memberEntityIds, isSystemAdmin);
    }
}
