using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Reference;

namespace REAK.Api.Models.Entities.Listings;

public class ListingAmenity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [Required]
    [ForeignKey(nameof(Amenity))]
    public Guid AmenityId { get; set; }

    public virtual PropertyListing Listing { get; set; } = null!;
    public virtual Amenity Amenity { get; set; } = null!;
}
