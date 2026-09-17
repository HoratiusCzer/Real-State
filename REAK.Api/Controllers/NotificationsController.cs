using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;

namespace REAK.Api.Controllers;

/// <summary>Notifications have no RLS yet (Stage 13 gap, consistent with Profile/MemberEntity) —
/// every query and mutation here is explicitly scoped to the caller's own ProfileId, never a bare
/// permission check, since "your own notifications" isn't a permission-gated concept.</summary>
[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(ReakDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var profileId = GetProfileId();
        var notifications = await db.Notifications
            .Where(n => n.ProfileId == profileId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new { n.Id, n.Type, n.Title, n.Body, n.LinkUrl, n.IsRead, n.CreatedAt })
            .ToListAsync(ct);

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        var profileId = GetProfileId();
        var count = await db.Notifications.CountAsync(n => n.ProfileId == profileId && !n.IsRead, ct);
        return Ok(new { count });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var profileId = GetProfileId();
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.ProfileId == profileId, ct);
        if (notification is null)
        {
            return NotFound();
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var profileId = GetProfileId();
        var unread = await db.Notifications.Where(n => n.ProfileId == profileId && !n.IsRead).ToListAsync(ct);
        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Guid GetProfileId() => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
