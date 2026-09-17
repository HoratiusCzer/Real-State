using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using REAK.Api.Services.Audit;

namespace REAK.Api.Services.Security;

/// <summary>Server-side permission check per action (spec §7) — checks the caller's JWT "perm"
/// claims (or the is_system_admin bypass) against a specific permission slug, never a role name.
/// This is a UX/API-layer convenience only; the database RLS in RowLevelSecurity.sql is the
/// actual authority regardless of what this attribute decides (spec §2.5).</summary>
public class RequirePermissionAttribute(string permissionSlug) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var isSystemAdmin = user.FindFirst(ClaimsNames.IsSystemAdmin)?.Value == "true";
        var hasPermission = isSystemAdmin || user.Claims.Any(c => c.Type == ClaimsNames.Permission && c.Value == permissionSlug);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();

            // Ordinary attribute filters aren't constructor-injected — resolved from
            // RequestServices instead (spec §33: "Log: ... authorization failures... security
            // events"). A caller probing for permissions they don't have is exactly the kind of
            // event this line exists to catch.
            var auditLogService = context.HttpContext.RequestServices.GetRequiredService<IAuditLogService>();
            var profileIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var actorProfileId = Guid.TryParse(profileIdClaim, out var id) ? id : (Guid?)null;
            await auditLogService.LogAsync(actorProfileId, "AuthorizationFailed", "Permission", null,
                $"Denied {permissionSlug} on {context.HttpContext.Request.Method} {context.HttpContext.Request.Path}.");
        }
    }
}
