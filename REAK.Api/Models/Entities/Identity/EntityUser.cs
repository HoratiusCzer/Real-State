using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>Join table linking a Profile to the MemberEntity it belongs to (spec §2.2).</summary>
public class EntityUser
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    [ForeignKey(nameof(MemberEntity))]
    public Guid MemberEntityId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile Profile { get; set; } = null!;
    public virtual MemberEntity MemberEntity { get; set; } = null!;
}
