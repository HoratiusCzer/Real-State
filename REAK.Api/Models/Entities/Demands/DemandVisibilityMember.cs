using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Demands;

public class DemandVisibilityMember
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [Required]
    [ForeignKey(nameof(MemberEntity))]
    public Guid MemberEntityId { get; set; }

    public virtual Demand Demand { get; set; } = null!;
    public virtual MemberEntity MemberEntity { get; set; } = null!;
}
