using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Services.Security;

public record UserClaims(bool IsSystemAdmin, IReadOnlyList<string> Permissions, IReadOnlyList<Guid> MemberEntityIds);

/// <summary>Resolves what a Profile is currently authorized to do — its permission slugs (spec §7,
/// never a bare role-name check), whether any of its role assignments is system-scoped (which the
/// RLS predicates in RowLevelSecurity.sql treat as a full bypass, matching that both system roles
/// are already granted every permission by DatabaseSeeder), and which member organizations it
/// belongs to. Computed fresh from the database at login/refresh time — never trust a stale JWT's
/// claims as the source of truth for a fast-changing grant, only as a cache of it.</summary>
public interface IUserClaimsFactory
{
    Task<UserClaims> BuildAsync(Guid profileId, CancellationToken ct = default);
}
