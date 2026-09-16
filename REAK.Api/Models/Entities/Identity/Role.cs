using System.ComponentModel.DataAnnotations;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Identity;

public class Role
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public RoleScope Scope { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<ProfileRoleAssignment> ProfileRoleAssignments { get; set; } = new List<ProfileRoleAssignment>();
}
