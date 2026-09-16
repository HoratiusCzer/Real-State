using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Cms;

public class NewsArticle
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    public string? Body { get; set; }

    [Required]
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    public DateTime? PublishedAt { get; set; }

    [Required]
    [ForeignKey(nameof(AuthorProfile))]
    public Guid AuthorProfileId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual Profile AuthorProfile { get; set; } = null!;
}
