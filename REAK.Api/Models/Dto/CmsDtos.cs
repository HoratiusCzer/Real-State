using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Dto;

// Shared by CmsPage/NewsArticle/Notice/Event/Resource's admin surface — the five content types
// that actually carry the Draft->Review->Published->Archived lifecycle (spec §4.3 Flow E).
// CommitteeMember/NavigationItem/SiteSetting are simpler (no lifecycle) and get their own DTOs.
public record UpsertPageRequest(
    [Required, MaxLength(150)] string Slug, [Required, MaxLength(255)] string Title, string? Body,
    [MaxLength(255)] string? SeoTitle, [MaxLength(500)] string? SeoDescription, [MaxLength(1000)] string? OgImageUrl);
public record PageDto(
    Guid Id, string Slug, string Title, string? Body, string Status, string? SeoTitle, string? SeoDescription,
    string? OgImageUrl, DateTime? PublishedAt, DateTime CreatedAt, DateTime? UpdatedAt);

public record UpsertNewsRequest([Required, MaxLength(150)] string Slug, [Required, MaxLength(255)] string Title, [MaxLength(500)] string? Summary, string? Body);
public record NewsDto(Guid Id, string Slug, string Title, string? Summary, string? Body, string Status, DateTime? PublishedAt, string AuthorProfileName, DateTime CreatedAt, DateTime? UpdatedAt);

public record UpsertNoticeRequest([Required, MaxLength(150)] string Slug, [Required, MaxLength(255)] string Title, string? Body);
public record NoticeDto(Guid Id, string Slug, string Title, string? Body, string Status, DateTime? PublishedAt, DateTime CreatedAt, DateTime? UpdatedAt);

public record UpsertEventRequest([Required, MaxLength(150)] string Slug, [Required, MaxLength(255)] string Title, string? Body, DateTime? EventDate, [MaxLength(500)] string? Location);
public record EventDto(Guid Id, string Slug, string Title, string? Body, DateTime? EventDate, string? Location, string Status, DateTime? PublishedAt, DateTime CreatedAt, DateTime? UpdatedAt);

public record UpsertResourceRequest([Required, MaxLength(255)] string Title, [MaxLength(1000)] string? Description, [MaxLength(1000)] string? LinkUrl);
public record ResourceDto(Guid Id, string Title, string? Description, string? LinkUrl, string Status, DateTime CreatedAt);

public record SetContentStatusRequest([Required] ContentStatus Status);

public record UpsertCommitteeMemberRequest([Required, MaxLength(255)] string Name, [Required, MaxLength(150)] string Title, [MaxLength(1000)] string? PhotoUrl, int SortOrder);
public record CommitteeMemberDto(Guid Id, string Name, string Title, string? PhotoUrl, int SortOrder, bool IsActive);

public record UpsertNavigationItemRequest([Required, MaxLength(100)] string Label, [Required, MaxLength(500)] string Url, Guid? ParentId, int SortOrder);
public record NavigationItemDto(Guid Id, string Label, string Url, Guid? ParentId, int SortOrder, bool IsActive);

public record UpsertSiteSettingRequest([Required, MaxLength(150)] string Key, [MaxLength(2000)] string? Value, [MaxLength(500)] string? Description);
public record SiteSettingDto(Guid Id, string Key, string? Value, string? Description, DateTime UpdatedAt);
