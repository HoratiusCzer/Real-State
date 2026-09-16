namespace REAK.Api.Services.Security;

/// <summary>The caller identity a service method needs to compute "is this my listing/demand"
/// (IsOwner) or "my org's" (MemberEntityId scoping) — extracted from JWT claims by
/// CallerContextFactory so services stay focused on data, not claim parsing. Not a substitute for
/// RLS: RLS is still what actually blocks an unauthorized read/write at the database layer
/// regardless of what this context says (spec §2.5). Shared across Listings and Demands since
/// both mirror the same ownership model (spec §2.3).</summary>
public record CallerContext(Guid ProfileId, IReadOnlyList<Guid> MemberEntityIds, bool IsSystemAdmin);
