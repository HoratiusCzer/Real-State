using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>A permission slug (e.g. "listings.moderate"). Server-side authorization checks against this,
/// never against a role name directly (spec §7).</summary>
public class Permission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
