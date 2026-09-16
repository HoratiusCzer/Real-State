using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Entities.Matching;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Collaboration;

/// <summary>Flow D step 1-2 (spec §2.4): Member B sends this, typically from a match; Member A accepts,
/// declines, or cancels it. Acceptance is what creates the CollaborationWorkspace — never automatic.</summary>
public class CollaborationRequest
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(Match))]
    public Guid? MatchId { get; set; }

    [Required]
    [ForeignKey(nameof(FromMemberEntity))]
    public Guid FromMemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(ToMemberEntity))]
    public Guid ToMemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(RequestedByProfile))]
    public Guid RequestedByProfileId { get; set; }

    [Required]
    public CollaborationRequestStatus Status { get; set; } = CollaborationRequestStatus.Pending;

    [MaxLength(1000)]
    public string? Message { get; set; }

    [ForeignKey(nameof(RespondedByProfile))]
    public Guid? RespondedByProfileId { get; set; }

    public DateTime? RespondedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Match? Match { get; set; }
    public virtual MemberEntity FromMemberEntity { get; set; } = null!;
    public virtual MemberEntity ToMemberEntity { get; set; } = null!;
    public virtual Profile RequestedByProfile { get; set; } = null!;
    public virtual Profile? RespondedByProfile { get; set; }
    public virtual CollaborationWorkspace? Workspace { get; set; }
}
