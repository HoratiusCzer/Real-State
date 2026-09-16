using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Collaboration;

public class CollaborationTask
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    [ForeignKey(nameof(CreatedByProfile))]
    public Guid CreatedByProfileId { get; set; }

    [ForeignKey(nameof(AssignedToProfile))]
    public Guid? AssignedToProfileId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    public DateTime? DueDate { get; set; }

    [Required]
    public CollaborationTaskStatus Status { get; set; } = CollaborationTaskStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile CreatedByProfile { get; set; } = null!;
    public virtual Profile? AssignedToProfile { get; set; }
}
