using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>Grants a Profile a Role. MemberEntityId is null for system-level roles (not tied to any
/// organization) and set for organization-level roles (spec §2.2).</summary>
public class ProfileRoleAssignment
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public Guid RoleId { get; set; }

    [ForeignKey(nameof(MemberEntity))]
    public Guid? MemberEntityId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile Profile { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual MemberEntity? MemberEntity { get; set; }
}
