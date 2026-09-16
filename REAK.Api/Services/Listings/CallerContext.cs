namespace REAK.Api.Services.Listings;

/// <summary>The caller identity a service method needs to compute "is this my listing" (IsOwner)
/// or "my org's" (MemberEntityId scoping) — extracted from JWT claims by the controller so
/// services stay focused on data, not claim parsing. Not a substitute for RLS: RLS is still what
/// actually blocks an unauthorized read/write at the database layer regardless of what this
/// context says (spec §2.5).</summary>
public record CallerContext(Guid ProfileId, IReadOnlyList<Guid> MemberEntityIds, bool IsSystemAdmin);
