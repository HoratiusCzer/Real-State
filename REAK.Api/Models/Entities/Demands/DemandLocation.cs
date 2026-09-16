using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Reference;

namespace REAK.Api.Models.Entities.Demands;

/// <summary>One acceptable location for a demand (a demand can accept several, at whatever hierarchy
/// depth the client cares about — e.g. "anywhere in this district" vs. "this specific ward").</summary>
public class DemandLocation
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [Required]
    [ForeignKey(nameof(Province))]
    public Guid ProvinceId { get; set; }

    [ForeignKey(nameof(District))]
    public Guid? DistrictId { get; set; }

    [ForeignKey(nameof(Municipality))]
    public Guid? MunicipalityId { get; set; }

    [ForeignKey(nameof(Ward))]
    public Guid? WardId { get; set; }

    [ForeignKey(nameof(Locality))]
    public Guid? LocalityId { get; set; }

    public virtual Demand Demand { get; set; } = null!;
    public virtual Province Province { get; set; } = null!;
    public virtual District? District { get; set; }
    public virtual Municipality? Municipality { get; set; }
    public virtual Ward? Ward { get; set; }
    public virtual Locality? Locality { get; set; }
}
