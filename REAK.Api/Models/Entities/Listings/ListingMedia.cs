using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>Public listing images — follow listing visibility, structurally separate from private
/// ListingDocument (spec §8.5).</summary>
public class ListingMedia
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }

    [MaxLength(255)]
    public string? Caption { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual PropertyListing Listing { get; set; } = null!;
}
