using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>The only mechanism by which a listing/demand contact becomes visible to the other
/// organization in a collaboration (spec §2.4 Flow D step 4, §13.2). Acceptance of the collaboration
/// request alone never creates one of these — it is always an explicit, separate, revocable grant.</summary>
public class CollaborationContactDisclosure
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Workspace))]
    public Guid CollaborationWorkspaceId { get; set; }

    [Required]
    public ContactDataType DataType { get; set; }

    [Required]
    [ForeignKey(nameof(GrantingMemberEntity))]
    public Guid GrantingMemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(ReceivingMemberEntity))]
    public Guid ReceivingMemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(GrantingProfile))]
    public Guid GrantingProfileId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAt { get; set; }

    [Required]
    [MaxLength(20)]
    public string PolicyVersion { get; set; } = "1.0";

    public virtual CollaborationWorkspace Workspace { get; set; } = null!;
    public virtual MemberEntity GrantingMemberEntity { get; set; } = null!;
    public virtual MemberEntity ReceivingMemberEntity { get; set; } = null!;
    public virtual Profile GrantingProfile { get; set; } = null!;
}
