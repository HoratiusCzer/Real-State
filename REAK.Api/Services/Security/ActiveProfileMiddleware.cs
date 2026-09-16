using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;

namespace REAK.Api.Services.Security;

/// <summary>"Suspension must revoke access immediately, even for an already-active session" (spec
/// §2.2). A short-lived JWT alone can't satisfy that — the token stays valid until it expires even
/// if the profile is suspended a second later. This re-checks Profile.IsActive against the
/// database on every authenticated request (a single indexed PK lookup) and rejects immediately if
/// it's false, rather than waiting for the access token to expire or the client to hit /refresh.</summary>
public class ActiveProfileMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ReakDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var profileIdClaim = context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(profileIdClaim, out var profileId))
            {
                var isActive = await db.Profiles
                    .Where(p => p.Id == profileId)
                    .Select(p => (bool?)p.IsActive)
                    .FirstOrDefaultAsync();

                if (isActive != true)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "Account is suspended or no longer exists." });
                    return;
                }
            }
        }

        await next(context);
    }
}
