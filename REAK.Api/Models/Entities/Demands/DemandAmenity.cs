using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Reference;

namespace REAK.Api.Models.Entities.Demands;

public class DemandAmenity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [Required]
    [ForeignKey(nameof(Amenity))]
    public Guid AmenityId { get; set; }

    public virtual Demand Demand { get; set; } = null!;
    public virtual Amenity Amenity { get; set; } = null!;
}
