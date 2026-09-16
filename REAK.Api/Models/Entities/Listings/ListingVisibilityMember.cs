using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>When Listing.NetworkVisibility == SelectedMembers, this join table enumerates which
/// member organizations may see it.</summary>
public class ListingVisibilityMember
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [Required]
    [ForeignKey(nameof(MemberEntity))]
    public Guid MemberEntityId { get; set; }

    public virtual PropertyListing Listing { get; set; } = null!;
    public virtual MemberEntity MemberEntity { get; set; } = null!;
}
