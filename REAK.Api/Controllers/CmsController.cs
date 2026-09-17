using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Entities.Cms;
using REAK.Api.Models.Enums;
using REAK.Api.Services.Notifications;
using REAK.Api.Services.Security;
using CmsEvent = REAK.Api.Models.Entities.Cms.Event;

namespace REAK.Api.Controllers;

/// <summary>Admin authoring for every content type in spec §4.3's "Admin CMS scope" (Flow E):
/// Draft -> Review -> Published -> Archived. None of these tables carry RLS (they're association-
/// wide editorial content, not per-organization private data, same reasoning as FeatureFlags) —
/// cms.manage is the only gate. PublicContentController is the other half: published-only,
/// sanitized reads for the public site, mirroring PublicPropertiesController's pattern from Stage 6
/// ("never let anonymous callers query the private/admin table directly").</summary>
[ApiController]
[Route("api/cms")]
[Authorize]
[RequirePermission("cms.manage")]
public class CmsController(ReakDbContext db, INotificationService notificationService) : ControllerBase
{
    // Sequential lifecycle, one step at a time in either direction — publishing skips straight to
    // Published only when the content was in Review, forcing every article through a review step
    // by convention (an admin can still Draft -> Review -> Published in two quick calls, but never
    // silently skip review by construction).
    private static readonly Dictionary<ContentStatus, ContentStatus[]> AllowedTransitions = new()
    {
        [ContentStatus.Draft] = [ContentStatus.Review],
        [ContentStatus.Review] = [ContentStatus.Draft, ContentStatus.Published],
        [ContentStatus.Published] = [ContentStatus.Archived],
        [ContentStatus.Archived] = [ContentStatus.Draft],
    };

    private Guid CallerProfileId => Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    // ---- Pages ----

    [HttpGet("pages")]
    public async Task<IActionResult> ListPages(CancellationToken ct) =>
        Ok(await db.CmsPages.OrderByDescending(p => p.CreatedAt).Select(p => ToDto(p)).ToListAsync(ct));

    [HttpGet("pages/{id:guid}")]
    public async Task<IActionResult> GetPage(Guid id, CancellationToken ct)
    {
        var page = await db.CmsPages.FirstOrDefaultAsync(p => p.Id == id, ct);
        return page is null ? NotFound() : Ok(ToDto(page));
    }

    [HttpPost("pages")]
    public async Task<IActionResult> CreatePage(UpsertPageRequest request, CancellationToken ct)
    {
        if (await db.CmsPages.AnyAsync(p => p.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A page with this slug already exists." });
        }

        var page = new CmsPage
        {
            Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Body = request.Body,
            SeoTitle = request.SeoTitle, SeoDescription = request.SeoDescription, OgImageUrl = request.OgImageUrl,
            CreatedByProfileId = CallerProfileId,
        };
        db.CmsPages.Add(page);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetPage), new { id = page.Id }, ToDto(page));
    }

    [HttpPut("pages/{id:guid}")]
    public async Task<IActionResult> UpdatePage(Guid id, UpsertPageRequest request, CancellationToken ct)
    {
        var page = await db.CmsPages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (page is null) return NotFound();
        if (await db.CmsPages.AnyAsync(p => p.Id != id && p.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A page with this slug already exists." });
        }

        page.Slug = request.Slug;
        page.Title = request.Title;
        page.Body = request.Body;
        page.SeoTitle = request.SeoTitle;
        page.SeoDescription = request.SeoDescription;
        page.OgImageUrl = request.OgImageUrl;
        page.UpdatedByProfileId = CallerProfileId;
        page.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(page));
    }

    [HttpPatch("pages/{id:guid}/status")]
    public async Task<IActionResult> SetPageStatus(Guid id, SetContentStatusRequest request, CancellationToken ct)
    {
        var page = await db.CmsPages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (page is null) return NotFound();
        if (!TryTransition(page.Status, request.Status, out var error)) return BadRequest(new { error });

        page.Status = request.Status;
        page.PublishedAt = request.Status == ContentStatus.Published ? DateTime.UtcNow : page.PublishedAt;
        page.UpdatedByProfileId = CallerProfileId;
        page.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("pages/{id:guid}")]
    public async Task<IActionResult> DeletePage(Guid id, CancellationToken ct)
    {
        var page = await db.CmsPages.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (page is null) return NotFound();
        if (page.Status != ContentStatus.Draft) return BadRequest(new { error = "Only a draft page can be deleted — archive it instead." });

        db.CmsPages.Remove(page);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- News ----

    [HttpGet("news")]
    public async Task<IActionResult> ListNews(CancellationToken ct) =>
        Ok(await db.NewsArticles.OrderByDescending(n => n.CreatedAt).Select(n => ToDto(n)).ToListAsync(ct));

    [HttpGet("news/{id:guid}")]
    public async Task<IActionResult> GetNews(Guid id, CancellationToken ct)
    {
        var article = await db.NewsArticles.Include(n => n.AuthorProfile).FirstOrDefaultAsync(n => n.Id == id, ct);
        return article is null ? NotFound() : Ok(ToDto(article));
    }

    [HttpPost("news")]
    public async Task<IActionResult> CreateNews(UpsertNewsRequest request, CancellationToken ct)
    {
        if (await db.NewsArticles.AnyAsync(n => n.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A news article with this slug already exists." });
        }

        var article = new NewsArticle
        {
            Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Summary = request.Summary, Body = request.Body,
            AuthorProfileId = CallerProfileId,
        };
        db.NewsArticles.Add(article);
        await db.SaveChangesAsync(ct);
        await db.Entry(article).Reference(n => n.AuthorProfile).LoadAsync(ct);
        return CreatedAtAction(nameof(GetNews), new { id = article.Id }, ToDto(article));
    }

    [HttpPut("news/{id:guid}")]
    public async Task<IActionResult> UpdateNews(Guid id, UpsertNewsRequest request, CancellationToken ct)
    {
        var article = await db.NewsArticles.Include(n => n.AuthorProfile).FirstOrDefaultAsync(n => n.Id == id, ct);
        if (article is null) return NotFound();
        if (await db.NewsArticles.AnyAsync(n => n.Id != id && n.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A news article with this slug already exists." });
        }

        article.Slug = request.Slug;
        article.Title = request.Title;
        article.Summary = request.Summary;
        article.Body = request.Body;
        article.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(article));
    }

    [HttpPatch("news/{id:guid}/status")]
    public async Task<IActionResult> SetNewsStatus(Guid id, SetContentStatusRequest request, CancellationToken ct)
    {
        var article = await db.NewsArticles.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (article is null) return NotFound();
        if (!TryTransition(article.Status, request.Status, out var error)) return BadRequest(new { error });

        article.Status = request.Status;
        article.PublishedAt = request.Status == ContentStatus.Published ? DateTime.UtcNow : article.PublishedAt;
        article.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("news/{id:guid}")]
    public async Task<IActionResult> DeleteNews(Guid id, CancellationToken ct)
    {
        var article = await db.NewsArticles.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (article is null) return NotFound();
        if (article.Status != ContentStatus.Draft) return BadRequest(new { error = "Only a draft article can be deleted — archive it instead." });

        db.NewsArticles.Remove(article);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Notices ----

    [HttpGet("notices")]
    public async Task<IActionResult> ListNotices(CancellationToken ct) =>
        Ok(await db.Notices.OrderByDescending(n => n.CreatedAt).Select(n => ToDto(n)).ToListAsync(ct));

    [HttpGet("notices/{id:guid}")]
    public async Task<IActionResult> GetNotice(Guid id, CancellationToken ct)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);
        return notice is null ? NotFound() : Ok(ToDto(notice));
    }

    [HttpPost("notices")]
    public async Task<IActionResult> CreateNotice(UpsertNoticeRequest request, CancellationToken ct)
    {
        if (await db.Notices.AnyAsync(n => n.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A notice with this slug already exists." });
        }

        var notice = new Notice { Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Body = request.Body, CreatedByProfileId = CallerProfileId };
        db.Notices.Add(notice);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetNotice), new { id = notice.Id }, ToDto(notice));
    }

    [HttpPut("notices/{id:guid}")]
    public async Task<IActionResult> UpdateNotice(Guid id, UpsertNoticeRequest request, CancellationToken ct)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notice is null) return NotFound();
        if (await db.Notices.AnyAsync(n => n.Id != id && n.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "A notice with this slug already exists." });
        }

        notice.Slug = request.Slug;
        notice.Title = request.Title;
        notice.Body = request.Body;
        notice.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(notice));
    }

    [HttpPatch("notices/{id:guid}/status")]
    public async Task<IActionResult> SetNoticeStatus(Guid id, SetContentStatusRequest request, CancellationToken ct)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notice is null) return NotFound();
        if (!TryTransition(notice.Status, request.Status, out var error)) return BadRequest(new { error });

        var wasPublished = notice.Status == ContentStatus.Published;
        notice.Status = request.Status;
        notice.PublishedAt = request.Status == ContentStatus.Published ? DateTime.UtcNow : notice.PublishedAt;
        notice.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        // Stage 10 deferred this exact trigger ("association notices") since nothing could publish
        // a Notice yet — this stage closes that gap. Broadcasting to every active profile (not
        // scoped to an org) is deliberate: a notice is association-wide by definition.
        if (request.Status == ContentStatus.Published && !wasPublished)
        {
            var allActiveProfileIds = await db.Profiles.Where(p => p.IsActive).Select(p => p.Id).ToListAsync(ct);
            await notificationService.NotifyManyAsync(allActiveProfileIds, NotificationType.AssociationNotice, notice.Title, null, $"/notices/{notice.Slug}", ct);
        }

        return NoContent();
    }

    [HttpDelete("notices/{id:guid}")]
    public async Task<IActionResult> DeleteNotice(Guid id, CancellationToken ct)
    {
        var notice = await db.Notices.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (notice is null) return NotFound();
        if (notice.Status != ContentStatus.Draft) return BadRequest(new { error = "Only a draft notice can be deleted — archive it instead." });

        db.Notices.Remove(notice);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Events ----

    [HttpGet("events")]
    public async Task<IActionResult> ListEvents(CancellationToken ct) =>
        Ok(await db.Events.OrderByDescending(e => e.CreatedAt).Select(e => ToDto(e)).ToListAsync(ct));

    [HttpGet("events/{id:guid}")]
    public async Task<IActionResult> GetEvent(Guid id, CancellationToken ct)
    {
        var evt = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        return evt is null ? NotFound() : Ok(ToDto(evt));
    }

    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent(UpsertEventRequest request, CancellationToken ct)
    {
        if (await db.Events.AnyAsync(e => e.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "An event with this slug already exists." });
        }

        var evt = new CmsEvent
        {
            Id = Guid.NewGuid(), Slug = request.Slug, Title = request.Title, Body = request.Body,
            EventDate = request.EventDate, Location = request.Location, CreatedByProfileId = CallerProfileId,
        };
        db.Events.Add(evt);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, ToDto(evt));
    }

    [HttpPut("events/{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, UpsertEventRequest request, CancellationToken ct)
    {
        var evt = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evt is null) return NotFound();
        if (await db.Events.AnyAsync(e => e.Id != id && e.Slug == request.Slug, ct))
        {
            return BadRequest(new { error = "An event with this slug already exists." });
        }

        evt.Slug = request.Slug;
        evt.Title = request.Title;
        evt.Body = request.Body;
        evt.EventDate = request.EventDate;
        evt.Location = request.Location;
        evt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(evt));
    }

    [HttpPatch("events/{id:guid}/status")]
    public async Task<IActionResult> SetEventStatus(Guid id, SetContentStatusRequest request, CancellationToken ct)
    {
        var evt = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evt is null) return NotFound();
        if (!TryTransition(evt.Status, request.Status, out var error)) return BadRequest(new { error });

        evt.Status = request.Status;
        evt.PublishedAt = request.Status == ContentStatus.Published ? DateTime.UtcNow : evt.PublishedAt;
        evt.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("events/{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken ct)
    {
        var evt = await db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (evt is null) return NotFound();
        if (evt.Status != ContentStatus.Draft) return BadRequest(new { error = "Only a draft event can be deleted — archive it instead." });

        db.Events.Remove(evt);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Resources ---- (same Draft -> Review -> Published -> Archived lifecycle as everything else, via the shared AllowedTransitions map — resources just have no title beyond the shared fields, no extra type-specific ones)

    [HttpGet("resources")]
    public async Task<IActionResult> ListResources(CancellationToken ct) =>
        Ok(await db.Resources.OrderByDescending(r => r.CreatedAt).Select(r => ToResourceDto(r)).ToListAsync(ct));

    [HttpPost("resources")]
    public async Task<IActionResult> CreateResource(UpsertResourceRequest request, CancellationToken ct)
    {
        var resource = new REAK.Api.Models.Entities.Cms.Resource
        {
            Id = Guid.NewGuid(), Title = request.Title, Description = request.Description, LinkUrl = request.LinkUrl,
            CreatedByProfileId = CallerProfileId,
        };
        db.Resources.Add(resource);
        await db.SaveChangesAsync(ct);
        return Ok(ToResourceDto(resource));
    }

    [HttpPut("resources/{id:guid}")]
    public async Task<IActionResult> UpdateResource(Guid id, UpsertResourceRequest request, CancellationToken ct)
    {
        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null) return NotFound();

        resource.Title = request.Title;
        resource.Description = request.Description;
        resource.LinkUrl = request.LinkUrl;
        await db.SaveChangesAsync(ct);
        return Ok(ToResourceDto(resource));
    }

    [HttpPatch("resources/{id:guid}/status")]
    public async Task<IActionResult> SetResourceStatus(Guid id, SetContentStatusRequest request, CancellationToken ct)
    {
        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null) return NotFound();
        if (!TryTransition(resource.Status, request.Status, out var error)) return BadRequest(new { error });

        resource.Status = request.Status;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("resources/{id:guid}")]
    public async Task<IActionResult> DeleteResource(Guid id, CancellationToken ct)
    {
        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (resource is null) return NotFound();

        db.Resources.Remove(resource);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Committee (no lifecycle — a roster, just active/inactive) ----

    [HttpGet("committee")]
    public async Task<IActionResult> ListCommittee(CancellationToken ct) =>
        Ok(await db.CommitteeMembers.OrderBy(c => c.SortOrder).Select(c => ToDto(c)).ToListAsync(ct));

    [HttpPost("committee")]
    public async Task<IActionResult> CreateCommitteeMember(UpsertCommitteeMemberRequest request, CancellationToken ct)
    {
        var member = new CommitteeMember { Id = Guid.NewGuid(), Name = request.Name, Title = request.Title, PhotoUrl = request.PhotoUrl, SortOrder = request.SortOrder };
        db.CommitteeMembers.Add(member);
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(member));
    }

    [HttpPut("committee/{id:guid}")]
    public async Task<IActionResult> UpdateCommitteeMember(Guid id, UpsertCommitteeMemberRequest request, CancellationToken ct)
    {
        var member = await db.CommitteeMembers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (member is null) return NotFound();

        member.Name = request.Name;
        member.Title = request.Title;
        member.PhotoUrl = request.PhotoUrl;
        member.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(member));
    }

    [HttpPost("committee/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleCommitteeMemberActive(Guid id, CancellationToken ct)
    {
        var member = await db.CommitteeMembers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (member is null) return NotFound();

        member.IsActive = !member.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(member));
    }

    [HttpDelete("committee/{id:guid}")]
    public async Task<IActionResult> DeleteCommitteeMember(Guid id, CancellationToken ct)
    {
        var member = await db.CommitteeMembers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (member is null) return NotFound();

        db.CommitteeMembers.Remove(member);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Navigation ----

    [HttpGet("navigation")]
    public async Task<IActionResult> ListNavigation(CancellationToken ct) =>
        Ok(await db.NavigationItems.OrderBy(n => n.SortOrder).Select(n => ToDto(n)).ToListAsync(ct));

    [HttpPost("navigation")]
    public async Task<IActionResult> CreateNavigationItem(UpsertNavigationItemRequest request, CancellationToken ct)
    {
        if (request.ParentId is not null && !await db.NavigationItems.AnyAsync(n => n.Id == request.ParentId, ct))
        {
            return BadRequest(new { error = "Parent navigation item not found." });
        }

        var item = new NavigationItem { Id = Guid.NewGuid(), Label = request.Label, Url = request.Url, ParentId = request.ParentId, SortOrder = request.SortOrder };
        db.NavigationItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(item));
    }

    [HttpPut("navigation/{id:guid}")]
    public async Task<IActionResult> UpdateNavigationItem(Guid id, UpsertNavigationItemRequest request, CancellationToken ct)
    {
        var item = await db.NavigationItems.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (item is null) return NotFound();
        if (request.ParentId == id) return BadRequest(new { error = "A navigation item cannot be its own parent." });

        item.Label = request.Label;
        item.Url = request.Url;
        item.ParentId = request.ParentId;
        item.SortOrder = request.SortOrder;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(item));
    }

    [HttpPost("navigation/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleNavigationItemActive(Guid id, CancellationToken ct)
    {
        var item = await db.NavigationItems.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (item is null) return NotFound();

        item.IsActive = !item.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(item));
    }

    [HttpDelete("navigation/{id:guid}")]
    public async Task<IActionResult> DeleteNavigationItem(Guid id, CancellationToken ct)
    {
        var item = await db.NavigationItems.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (item is null) return NotFound();
        if (await db.NavigationItems.AnyAsync(n => n.ParentId == id, ct))
        {
            return BadRequest(new { error = "Remove or reassign this item's children first." });
        }

        db.NavigationItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    // ---- Site settings (plain key/value, no lifecycle — effective immediately, same as FeatureFlags) ----

    [HttpGet("settings")]
    public async Task<IActionResult> ListSettings(CancellationToken ct) =>
        Ok(await db.SiteSettings.OrderBy(s => s.Key).Select(s => ToDto(s)).ToListAsync(ct));

    [HttpPut("settings")]
    public async Task<IActionResult> UpsertSetting(UpsertSiteSettingRequest request, CancellationToken ct)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Key == request.Key, ct);
        if (setting is null)
        {
            setting = new SiteSetting { Id = Guid.NewGuid(), Key = request.Key };
            db.SiteSettings.Add(setting);
        }

        setting.Value = request.Value;
        setting.Description = request.Description;
        setting.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ToDto(setting));
    }

    [HttpDelete("settings/{id:guid}")]
    public async Task<IActionResult> DeleteSetting(Guid id, CancellationToken ct)
    {
        var setting = await db.SiteSettings.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (setting is null) return NotFound();

        db.SiteSettings.Remove(setting);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static bool TryTransition(ContentStatus from, ContentStatus to, out string? error)
    {
        if (AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to))
        {
            error = null;
            return true;
        }

        error = $"Cannot move content from {from} to {to}.";
        return false;
    }

    private static PageDto ToDto(CmsPage p) => new(p.Id, p.Slug, p.Title, p.Body, p.Status.ToString(), p.SeoTitle, p.SeoDescription, p.OgImageUrl, p.PublishedAt, p.CreatedAt, p.UpdatedAt);
    private static NewsDto ToDto(NewsArticle n) => new(n.Id, n.Slug, n.Title, n.Summary, n.Body, n.Status.ToString(), n.PublishedAt, n.AuthorProfile?.FullName ?? string.Empty, n.CreatedAt, n.UpdatedAt);
    private static NoticeDto ToDto(Notice n) => new(n.Id, n.Slug, n.Title, n.Body, n.Status.ToString(), n.PublishedAt, n.CreatedAt, n.UpdatedAt);
    private static EventDto ToDto(CmsEvent e) => new(e.Id, e.Slug, e.Title, e.Body, e.EventDate, e.Location, e.Status.ToString(), e.PublishedAt, e.CreatedAt, e.UpdatedAt);
    private static ResourceDto ToResourceDto(REAK.Api.Models.Entities.Cms.Resource r) => new(r.Id, r.Title, r.Description, r.LinkUrl, r.Status.ToString(), r.CreatedAt);
    private static CommitteeMemberDto ToDto(CommitteeMember c) => new(c.Id, c.Name, c.Title, c.PhotoUrl, c.SortOrder, c.IsActive);
    private static NavigationItemDto ToDto(NavigationItem n) => new(n.Id, n.Label, n.Url, n.ParentId, n.SortOrder, n.IsActive);
    private static SiteSettingDto ToDto(SiteSetting s) => new(s.Id, s.Key, s.Value, s.Description, s.UpdatedAt);
}
