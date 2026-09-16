using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Admin-configurable amenity (e.g. Parking, Elevator), referenced by listing_amenities and
/// demand_amenities join tables.</summary>
public class Amenity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
