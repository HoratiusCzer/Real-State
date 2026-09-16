using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Collaboration;

public class CollaborationParticipant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    [ForeignKey(nameof(MemberEntity))]
    public Guid MemberEntityId { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual Profile Profile { get; set; } = null!;
    public virtual MemberEntity MemberEntity { get; set; } = null!;
}
