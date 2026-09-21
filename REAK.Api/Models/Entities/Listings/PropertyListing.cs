using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Entities.Reference;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Listings;

/// <summary>A property listing (spec §8.3). Public projection queries must gate on: public feature
/// enabled AND Status == Approved AND IsPublicVisible AND MemberEntity.IsActive AND !IsDeleted AND
/// (ExpiresAt is null or in the future) — never expose this table directly to anonymous callers.</summary>
public class PropertyListing
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>Server-generated, never supplied by the client (spec §21).</summary>
    [Required]
    [MaxLength(30)]
    public string ReferenceCode { get; set; } = string.Empty;

    [Required]
    [ForeignKey(nameof(MemberEntity))]
    public Guid MemberEntityId { get; set; }

    [Required]
    [ForeignKey(nameof(CreatedByProfile))]
    public Guid CreatedByProfileId { get; set; }

    [ForeignKey(nameof(UpdatedByProfile))]
    public Guid? UpdatedByProfileId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>Internal notes — never exposed outside the owning organization.</summary>
    [MaxLength(2000)]
    public string? InternalNotes { get; set; }

    [Required]
    [ForeignKey(nameof(PropertyType))]
    public Guid PropertyTypeId { get; set; }

    [ForeignKey(nameof(PropertySubtype))]
    public Guid? PropertySubtypeId { get; set; }

    [Required]
    [ForeignKey(nameof(Purpose))]
    public Guid PurposeId { get; set; }

    // Location (normalized Nepal hierarchy, spec §10 — not free-text)
    [Required]
    [ForeignKey(nameof(Province))]
    public Guid ProvinceId { get; set; }

    [Required]
    [ForeignKey(nameof(District))]
    public Guid DistrictId { get; set; }

    [Required]
    [ForeignKey(nameof(Municipality))]
    public Guid MunicipalityId { get; set; }

    [Required]
    [ForeignKey(nameof(Ward))]
    public Guid WardId { get; set; }

    [ForeignKey(nameof(Locality))]
    public Guid? LocalityId { get; set; }

    [MaxLength(255)]
    public string? Landmark { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal? Longitude { get; set; }

    // Price
    [Required]
    [ForeignKey(nameof(Currency))]
    public Guid CurrencyId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public bool IsPriceNegotiable { get; set; }

    // Area (spec §11). LandArea/AreaUnitId stay populated for backward compatibility — for a
    // RopaniSystem/BighaSystem entry they're derived server-side from the compound values below
    // (never trusted from the client), for SquareFeet/SquareMetres they're the direct entry.
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal LandArea { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? BuiltUpArea { get; set; }

    [Required]
    [ForeignKey(nameof(AreaUnit))]
    public Guid AreaUnitId { get; set; }

    [Required]
    public LandAreaMeasurementSystem MeasurementSystem { get; set; } = LandAreaMeasurementSystem.SquareFeet;

    // Only the set matching MeasurementSystem is populated — the exact original compound entry,
    // preserved so re-opening a listing to edit shows it exactly, not a lossy decimal
    // reconstruction (e.g. LandArea alone can't distinguish "4 Ropani 0 Aana 2 Paisa" from any
    // other combination that happens to total the same decimal).
    public int? RopaniValue { get; set; }
    public int? AanaValue { get; set; }
    public int? PaisaValue { get; set; }
    public int? DamValue { get; set; }

    public int? BighaValue { get; set; }
    public int? KatthaValue { get; set; }
    public int? DhurValue { get; set; }

    /// <summary>Canonical, unit-independent area — always computed server-side (never trusted
    /// from the client), used for cross-listing filtering/sorting/comparison (ListingSearchQuery,
    /// MatchingEngine's Area rule) regardless of which system the agent entered in.</summary>
    [Required]
    [Column(TypeName = "decimal(18,4)")]
    public decimal AreaInSquareFeet { get; set; }

    // Specifications
    public bool HasRoadAccess { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? RoadWidthFeet { get; set; }

    [MaxLength(100)]
    public string? RoadType { get; set; }

    public PropertyFacing? Facing { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? Floors { get; set; }

    public int? ParkingSpaces { get; set; }

    public Furnishing? Furnishing { get; set; }

    // Visibility (network and public are separate gates, spec §2.4)
    [Required]
    public NetworkVisibility NetworkVisibility { get; set; } = NetworkVisibility.OwnerOnly;

    public bool IsPublicVisible { get; set; }

    // Lifecycle (Flow B, spec §2.4)
    [Required]
    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    [ForeignKey(nameof(ApprovedByProfile))]
    public Guid? ApprovedByProfileId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual MemberEntity MemberEntity { get; set; } = null!;
    public virtual Profile CreatedByProfile { get; set; } = null!;
    public virtual Profile? UpdatedByProfile { get; set; }
    public virtual Profile? ApprovedByProfile { get; set; }
    public virtual PropertyType PropertyType { get; set; } = null!;
    public virtual PropertySubtype? PropertySubtype { get; set; }
    public virtual Purpose Purpose { get; set; } = null!;
    public virtual Reference.Province Province { get; set; } = null!;
    public virtual Reference.District District { get; set; } = null!;
    public virtual Reference.Municipality Municipality { get; set; } = null!;
    public virtual Reference.Ward Ward { get; set; } = null!;
    public virtual Reference.Locality? Locality { get; set; }
    public virtual Currency Currency { get; set; } = null!;
    public virtual AreaUnit AreaUnit { get; set; } = null!;

    public virtual ICollection<ListingMedia> Media { get; set; } = new List<ListingMedia>();
    public virtual ICollection<ListingDocument> Documents { get; set; } = new List<ListingDocument>();
    public virtual ICollection<ListingAmenity> ListingAmenities { get; set; } = new List<ListingAmenity>();
    public virtual ListingContact? Contact { get; set; }
    public virtual ICollection<ListingVisibilityMember> VisibilityMembers { get; set; } = new List<ListingVisibilityMember>();
}
