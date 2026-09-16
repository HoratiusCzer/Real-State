using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Collaboration;

public class CollaborationMessage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    [ForeignKey(nameof(SenderProfile))]
    public Guid SenderProfileId { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile SenderProfile { get; set; } = null!;
}
