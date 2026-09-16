using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace REAK.Api.Services.Security;

/// <summary>Server-side permission check per action (spec §7) — checks the caller's JWT "perm"
/// claims (or the is_system_admin bypass) against a specific permission slug, never a role name.
/// This is a UX/API-layer convenience only; the database RLS in RowLevelSecurity.sql is the
/// actual authority regardless of what this attribute decides (spec §2.5).</summary>
public class RequirePermissionAttribute(string permissionSlug) : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var isSystemAdmin = user.FindFirst(ClaimsNames.IsSystemAdmin)?.Value == "true";
        var hasPermission = isSystemAdmin || user.Claims.Any(c => c.Type == ClaimsNames.Permission && c.Value == permissionSlug);

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}
