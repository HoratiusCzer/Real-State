using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REAK.Api.Data;
using REAK.Api.Models.Dto;
using REAK.Api.Models.Enums;
using CmsEvent = REAK.Api.Models.Entities.Cms.Event;

namespace REAK.Api.Controllers;

/// <summary>The public half of the CMS (spec §4.1, §16): anonymous callers only ever see content
/// that is genuinely Published, via an explicit field allowlist — never a direct query against
/// the same tables CmsController edits. None of these tables carry RLS (they're association-wide
/// editorial content, not per-org private data), so this explicit Status == Published filter is
/// the only gate standing between a Draft/Review article and the public internet; it must stay
/// correct in every method here, the same discipline PublicPropertiesController uses for §16.</summary>
[ApiController]
[Route("api/public")]
[AllowAnonymous]
public class PublicContentController(ReakDbContext db) : ControllerBase
{
    [HttpGet("pages/{slug}")]
    public async Task<IActionResult> GetPage(string slug, CancellationToken ct)
    {
        var page = await db.CmsPages
            .Where(p => p.Slug == slug && p.Status == ContentStatus.Published)
            .Select(p => new { p.Slug, p.Title, p.Body, p.SeoTitle, p.SeoDescription, p.OgImageUrl, p.PublishedAt })
            .FirstOrDefaultAsync(ct);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpGet("news")]
    public async Task<IActionResult> ListNews(CancellationToken ct) =>
        Ok(await db.NewsArticles
            .Where(n => n.Status == ContentStatus.Published)
            .OrderByDescending(n => n.PublishedAt)
            .Select(n => new { n.Slug, n.Title, n.Summary, n.PublishedAt })
            .ToListAsync(ct));

    [HttpGet("news/{slug}")]
    public async Task<IActionResult> GetNews(string slug, CancellationToken ct)
    {
        var article = await db.NewsArticles
            .Where(n => n.Slug == slug && n.Status == ContentStatus.Published)
            .Select(n => new { n.Slug, n.Title, n.Summary, n.Body, n.PublishedAt, AuthorName = n.AuthorProfile.FullName })
            .FirstOrDefaultAsync(ct);
        return article is null ? NotFound() : Ok(article);
    }

    [HttpGet("notices")]
    public async Task<IActionResult> ListNotices(CancellationToken ct) =>
        Ok(await db.Notices
            .Where(n => n.Status == ContentStatus.Published)
            .OrderByDescending(n => n.PublishedAt)
            .Select(n => new { n.Slug, n.Title, n.PublishedAt })
            .ToListAsync(ct));

    [HttpGet("notices/{slug}")]
    public async Task<IActionResult> GetNotice(string slug, CancellationToken ct)
    {
        var notice = await db.Notices
            .Where(n => n.Slug == slug && n.Status == ContentStatus.Published)
            .Select(n => new { n.Slug, n.Title, n.Body, n.PublishedAt })
            .FirstOrDefaultAsync(ct);
        return notice is null ? NotFound() : Ok(notice);
    }

    [HttpGet("events")]
    public async Task<IActionResult> ListEvents(CancellationToken ct) =>
        Ok(await db.Events
            .Where(e => e.Status == ContentStatus.Published)
            .OrderBy(e => e.EventDate)
            .Select(e => new { e.Slug, e.Title, e.EventDate, e.Location })
            .ToListAsync(ct));

    [HttpGet("events/{slug}")]
    public async Task<IActionResult> GetEvent(string slug, CancellationToken ct)
    {
        var evt = await db.Events
            .Where(e => e.Slug == slug && e.Status == ContentStatus.Published)
            .Select(e => new { e.Slug, e.Title, e.Body, e.EventDate, e.Location, e.PublishedAt })
            .FirstOrDefaultAsync(ct);
        return evt is null ? NotFound() : Ok(evt);
    }

    [HttpGet("resources")]
    public async Task<IActionResult> ListResources(CancellationToken ct) =>
        Ok(await db.Resources
            .Where(r => r.Status == ContentStatus.Published)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new { r.Id, r.Title, r.Description, r.LinkUrl })
            .ToListAsync(ct));

    [HttpGet("committee")]
    public async Task<IActionResult> ListCommittee(CancellationToken ct) =>
        Ok(await db.CommitteeMembers
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new { c.Name, c.Title, c.PhotoUrl })
            .ToListAsync(ct));

    [HttpGet("navigation")]
    public async Task<IActionResult> ListNavigation(CancellationToken ct)
    {
        var items = await db.NavigationItems
            .Where(n => n.IsActive)
            .OrderBy(n => n.SortOrder)
            .Select(n => new { n.Id, n.Label, n.Url, n.ParentId })
            .ToListAsync(ct);
        return Ok(items);
    }
}
