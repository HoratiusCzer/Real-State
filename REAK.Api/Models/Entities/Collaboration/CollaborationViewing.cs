using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>A scheduled property viewing coordinated within the workspace.</summary>
public class CollaborationViewing
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    [ForeignKey(nameof(ScheduledByProfile))]
    public Guid ScheduledByProfileId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile ScheduledByProfile { get; set; } = null!;
}
