using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Entities.Collaboration;
using REAK.Api.Models.Entities.Notifications;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>A person. May belong to zero or more member organizations via EntityUser, and holds
/// roles (system-level, not tied to an org, or organization-level, scoped to one) via ProfileRoleAssignment.</summary>
public class Profile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>False means the account is suspended — access must be revoked immediately, even for an existing session (spec §2.2).</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<EntityUser> EntityMemberships { get; set; } = new List<EntityUser>();
    public virtual ICollection<ProfileRoleAssignment> RoleAssignments { get; set; } = new List<ProfileRoleAssignment>();
    public virtual ICollection<Entities.Notifications.Notification> Notifications { get; set; } = new List<Entities.Notifications.Notification>();
    public virtual ICollection<CollaborationParticipant> CollaborationParticipations { get; set; } = new List<CollaborationParticipant>();
}
