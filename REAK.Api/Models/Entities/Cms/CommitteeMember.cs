using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Cms;

/// <summary>Leadership/committee roster shown on the public site. Structure only — never seeded with
/// invented names or titles (spec §22); real entries are added by an admin.</summary>
public class CommitteeMember
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
