namespace REAK.Api.Services.Audit;

/// <summary>Append-only audit trail (spec §19: login/security changes, role changes, member
/// changes, listing changes, demand changes, moderation actions, collaboration events, contact
/// disclosure, feature flag changes, configuration changes). Deliberately parallel to
/// INotificationService — a small, self-contained, fire-and-forget write that never participates
/// in the caller's own transaction, so a logging failure can never roll back the action it's
/// describing. Summary must stay short and must never carry a full record dump or contact info
/// (spec's explicit warning) — every call site here passes a one-line human summary, not a
/// serialized entity.</summary>
public interface IAuditLogService
{
    Task LogAsync(Guid? actorProfileId, string action, string entityType, Guid? entityId, string? summary, CancellationToken ct = default);
}
