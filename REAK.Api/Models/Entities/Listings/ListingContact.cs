using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>Structurally isolated contact info for a listing (spec §2.3, §8.4). Must never be joined
/// into normal read queries. RLS-locked to the owning organization by default; visible to another
/// organization only via an explicit CollaborationContactDisclosure grant. One-to-one with the listing
/// on purpose, so it can never accidentally be pulled in by a naive `Include(l => l.Contact)` on a
/// public-facing query path without that path visibly opting in.</summary>
public class ListingContact
{
    [Key]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [MaxLength(255)]
    public string? ContactName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    public virtual PropertyListing Listing { get; set; } = null!;
}
