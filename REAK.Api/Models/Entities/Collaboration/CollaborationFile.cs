using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>Collaboration files get their own storage area, separate from listing/CMS media (spec §18).</summary>
public class CollaborationFile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    [ForeignKey(nameof(UploadedByProfile))]
    public Guid UploadedByProfileId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string StoragePath { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile UploadedByProfile { get; set; } = null!;
}
