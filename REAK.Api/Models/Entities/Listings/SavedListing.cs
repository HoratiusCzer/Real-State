using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>"Saved properties" (spec §8.1 Discovery). Per-profile, not per-organization — two
/// staff at the same member org each keep their own saved list. No RLS (same documented gap as
/// Notifications) — every query is explicitly scoped to the caller's own ProfileId in
/// application code, since "my saved list" isn't a permission-gated concept.</summary>
public class SavedListing
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile Profile { get; set; } = null!;
    public virtual PropertyListing Listing { get; set; } = null!;
}
