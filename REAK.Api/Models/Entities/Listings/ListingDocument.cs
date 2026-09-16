using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>Private documents — require signed/private access, upload/delete restricted to authorized
/// users, RLS-locked to the owning organization (spec §8.5). Do not assume specific document types
/// (citizenship, Lalpurja, PAN, etc.) unless an admin has configured them — DocumentType is free text.</summary>
public class ListingDocument
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Listing))]
    public Guid ListingId { get; set; }

    [MaxLength(100)]
    public string? DocumentType { get; set; }

    [Required]
    [MaxLength(1000)]
    public string StoragePath { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(UploadedByProfile))]
    public Guid UploadedByProfileId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual PropertyListing Listing { get; set; } = null!;
    public virtual Profile UploadedByProfile { get; set; } = null!;
}
