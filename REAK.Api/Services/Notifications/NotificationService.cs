using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Entities.Notifications;
using REAK.Api.Models.Enums;

namespace REAK.Api.Services.Notifications;

public class NotificationService(ReakDbContext db) : INotificationService
{
    public async Task NotifyAsync(Guid profileId, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default) =>
        await NotifyManyAsync([profileId], type, title, body, linkUrl, ct);

    public async Task NotifyManyAsync(IEnumerable<Guid> profileIds, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default)
    {
        var ids = profileIds.Distinct().ToList();
        if (ids.Count == 0) return;

        foreach (var profileId in ids)
        {
            db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId,
                Type = type,
                Title = title,
                Body = body,
                LinkUrl = linkUrl,
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task NotifyOrgAsync(Guid memberEntityId, NotificationType type, string title, string? body = null, string? linkUrl = null, CancellationToken ct = default)
    {
        var profileIds = await db.EntityUsers
            .Where(eu => eu.MemberEntityId == memberEntityId && eu.IsActive)
            .Select(eu => eu.ProfileId)
            .ToListAsync(ct);

        await NotifyManyAsync(profileIds, type, title, body, linkUrl, ct);
    }
}
