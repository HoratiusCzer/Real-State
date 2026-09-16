using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Enums;

namespace REAK.Api.Services.Security;

public class UserClaimsFactory(ReakDbContext db) : IUserClaimsFactory
{
    public async Task<UserClaims> BuildAsync(Guid profileId, CancellationToken ct = default)
    {
        var assignments = await db.ProfileRoleAssignments
            .Where(a => a.ProfileId == profileId)
            .Select(a => new { a.RoleId, a.Role.Scope })
            .ToListAsync(ct);

        var isSystemAdmin = assignments.Any(a => a.Scope == RoleScope.System);

        var roleIds = assignments.Select(a => a.RoleId).Distinct().ToList();
        var permissions = await db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Slug)
            .Distinct()
            .ToListAsync(ct);

        var memberEntityIds = await db.EntityUsers
            .Where(eu => eu.ProfileId == profileId && eu.IsActive)
            .Select(eu => eu.MemberEntityId)
            .ToListAsync(ct);

        return new UserClaims(isSystemAdmin, permissions, memberEntityIds);
    }
}
