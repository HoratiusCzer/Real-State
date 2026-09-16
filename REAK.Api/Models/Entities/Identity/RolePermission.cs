using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Identity;

public class RolePermission
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Role))]
    public Guid RoleId { get; set; }

    [Required]
    [ForeignKey(nameof(Permission))]
    public Guid PermissionId { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
