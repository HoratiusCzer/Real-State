using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Security;

namespace REAK.Api.Services.Notifications;

/// <summary>Spec §14's "expiry" notification trigger: PropertyListing/Demand both carry an
/// ExpiresAt (Stage 3 schema) that nothing ever actually acted on until now — ListingStatus.Expired
/// existed in the enum unused, and DemandStatus had no Expired value at all (added this stage,
/// mirroring Listing's model; additive, no check constraint or migration required).
///
/// This is a genuine background job — no HTTP caller, no JWT, nothing for
/// SessionContextConnectionInterceptor to project — so it must elevate the same way MatchingEngine
/// does (SessionContextOverride) to see across every organization, and it owns its own DI scope per
/// tick since BackgroundService is a singleton but ReakDbContext/SessionContextOverride/
/// INotificationService are all scoped.
///
/// An expired listing/demand simply stops appearing in MatchesController's live-match view (it
/// already filters to Approved listings / Active demands) — no explicit Match cleanup needed here,
/// same as ListingService.ArchiveAsync never touches Matches either.</summary>
public class ExpiryScanService(IServiceScopeFactory scopeFactory, ILogger<ExpiryScanService> logger) : BackgroundService
{
    // Hourly is frequent enough that nothing sits expired-but-unmarked for long in a live system,
    // without being so frequent it's pointless overhead — not a spec-mandated number, a reasonable
    // default adjustable later if real usage patterns call for something else.
    private static readonly TimeSpan ScanInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(ScanInterval);
        do
        {
            try
            {
                await ScanAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Expiry scan failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ScanAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ReakDbContext>();
        var sessionContextOverride = scope.ServiceProvider.GetRequiredService<SessionContextOverride>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        sessionContextOverride.IsSystemLevel = true;
        try
        {
            var now = DateTime.UtcNow;

            var expiredListings = await db.PropertyListings
                .Where(l => l.Status == ListingStatus.Approved && !l.IsDeleted && l.ExpiresAt != null && l.ExpiresAt <= now)
                .ToListAsync(ct);
            foreach (var listing in expiredListings)
            {
                listing.Status = ListingStatus.Expired;
            }

            var expiredDemands = await db.Demands
                .Where(d => d.Status == DemandStatus.Active && !d.IsDeleted && d.ExpiresAt != null && d.ExpiresAt <= now)
                .ToListAsync(ct);
            foreach (var demand in expiredDemands)
            {
                demand.Status = DemandStatus.Expired;
            }

            if (expiredListings.Count > 0 || expiredDemands.Count > 0)
            {
                await db.SaveChangesAsync(ct);
            }

            foreach (var listing in expiredListings)
            {
                await notificationService.NotifyAsync(listing.CreatedByProfileId, NotificationType.Expiry, $"\"{listing.Title}\" has expired", null, $"/portal/properties/{listing.Id}", ct);
            }
            foreach (var demand in expiredDemands)
            {
                await notificationService.NotifyAsync(demand.CreatedByProfileId, NotificationType.Expiry, $"\"{demand.Title}\" has expired", null, $"/portal/demands/{demand.Id}", ct);
            }
        }
        finally
        {
            sessionContextOverride.IsSystemLevel = false;
        }
    }
}
