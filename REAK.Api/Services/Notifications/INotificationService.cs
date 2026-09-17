using REAK.Api.Models.Enums;

namespace REAK.Api.Services.Notifications;

/// <summary>In-app notifications (spec §14). Deliberately parallel to IEmailSender: a small,
/// self-contained fire-and-forget write, not a transactional participant in the caller's own
/// SaveChangesAsync — a notification failing shouldn't be able to roll back the business action
/// that triggered it, and every call site here fires only after its own triggering change is
/// already committed.</summary>
public interface INotificationService
{
    Task NotifyAsync(Guid profileId, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default);

    Task NotifyManyAsync(IEnumerable<Guid> profileIds, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default);

    /// <summary>Notifies every active user of a member organization — the recipient side of an
    /// event is "this organization", not a single known profile (e.g. a collaboration request
    /// sent to an org, a new match for an org's listing).</summary>
    Task NotifyOrgAsync(Guid memberEntityId, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default);
}
