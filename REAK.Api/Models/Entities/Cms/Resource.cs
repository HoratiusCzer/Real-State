using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Cms;

public class Resource
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(1000)]
    public string? LinkUrl { get; set; }

    [Required]
    public ContentStatus Status { get; set; } = ContentStatus.Draft;

    [Required]
    [ForeignKey(nameof(CreatedByProfile))]
    public Guid CreatedByProfileId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile CreatedByProfile { get; set; } = null!;
}
