using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>Flow A step 3: admin creates an invitation; invitee accepts, sets a password, and
/// account activates (spec §2.4).</summary>
public class Invitation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Null for a system-level role invitation; set for an org-level role.</summary>
    [ForeignKey(nameof(MemberEntity))]
    public Guid? MemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public Guid RoleId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = string.Empty;

    [Required]
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    [Required]
    [ForeignKey(nameof(InvitedByProfile))]
    public Guid InvitedByProfileId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? AcceptedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual MemberEntity? MemberEntity { get; set; }
    public virtual Role Role { get; set; } = null!;
    public virtual Profile InvitedByProfile { get; set; } = null!;
}
