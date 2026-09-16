using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Cms;

/// <summary>A static/editorial page (homepage, about, mission, vision, footer, navigation copy,
/// legal pages, etc. — spec §4.3 Admin CMS scope). Draft -> Review -> Published -> Archived (Flow E).
/// Published content is what the public site renders; there is no direct "live edit" of public output.</summary>
public class CmsPage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    [Required]
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    [MaxLength(255)]
    public string? SeoTitle { get; set; }

    [MaxLength(500)]
    public string? SeoDescription { get; set; }

    [MaxLength(1000)]
    public string? OgImageUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    [Required]
    [ForeignKey(nameof(CreatedByProfile))]
    public Guid CreatedByProfileId { get; set; }

    [ForeignKey(nameof(UpdatedByProfile))]
    public Guid? UpdatedByProfileId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual Profile CreatedByProfile { get; set; } = null!;
    public virtual Profile? UpdatedByProfile { get; set; }
}
