using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>Append-only activity history for a workspace (e.g. "participant joined", "file uploaded").
/// ProfileId is null for system-generated entries.</summary>
public class CollaborationActivity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [ForeignKey(nameof(Profile))]
    public Guid? ProfileId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Action { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile? Profile { get; set; }
}
