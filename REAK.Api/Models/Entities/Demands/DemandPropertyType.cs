using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Reference;

namespace REAK.Api.Models.Entities.Demands;

/// <summary>A demand can accept more than one property type — proper join table, not an array (spec §9).</summary>
public class DemandPropertyType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [Required]
    [ForeignKey(nameof(PropertyType))]
    public Guid PropertyTypeId { get; set; }

    public virtual Demand Demand { get; set; } = null!;
    public virtual PropertyType PropertyType { get; set; } = null!;
}
