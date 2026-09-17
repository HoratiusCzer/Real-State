using REAK.Api.Data;
using REAK.Api.Models.Entities.Audit;

namespace REAK.Api.Services.Audit;

public class AuditLogService(ReakDbContext db, IHttpContextAccessor httpContextAccessor) : IAuditLogService
{
    public async Task LogAsync(Guid? actorProfileId, string action, string entityType, Guid? entityId, string? summary, CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorProfileId = actorProfileId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
        });
        await db.SaveChangesAsync(ct);
    }
}
