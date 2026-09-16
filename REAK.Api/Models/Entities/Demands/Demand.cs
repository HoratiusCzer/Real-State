using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Entities.Reference;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Demands;

/// <summary>A client requirement (spec §9) — mirrors PropertyListing structurally since the matching
/// engine treats listings and demands as two sides of one relationship. The client's identity lives
/// only in DemandContact, isolated the same way ListingContact is.</summary>
public class Demand
{
    [Key]
    public Guid Id { get; set; }

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

    [MaxLength(2000)]
    public string? InternalNotes { get; set; }

    [Required]
    [ForeignKey(nameof(Purpose))]
    public Guid PurposeId { get; set; }

    [ForeignKey(nameof(Currency))]
    public Guid? CurrencyId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinBudget { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxBudget { get; set; }

    [ForeignKey(nameof(AreaUnit))]
    public Guid? AreaUnitId { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MinArea { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? MaxArea { get; set; }

    public int? MinBedrooms { get; set; }

    public int? MinBathrooms { get; set; }

    [Required]
    public NetworkVisibility NetworkVisibility { get; set; } = NetworkVisibility.OwnerOnly;

    [Required]
    public DemandStatus Status { get; set; } = DemandStatus.Draft;

    public DateTime? ExpiresAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual MemberEntity MemberEntity { get; set; } = null!;
    public virtual Profile CreatedByProfile { get; set; } = null!;
    public virtual Profile? UpdatedByProfile { get; set; }
    public virtual Purpose Purpose { get; set; } = null!;
    public virtual Currency? Currency { get; set; }
    public virtual AreaUnit? AreaUnit { get; set; }

    public virtual ICollection<DemandPropertyType> PropertyTypes { get; set; } = new List<DemandPropertyType>();
    public virtual ICollection<DemandLocation> Locations { get; set; } = new List<DemandLocation>();
    public virtual ICollection<DemandAmenity> DemandAmenities { get; set; } = new List<DemandAmenity>();
    public virtual DemandContact? Contact { get; set; }
    public virtual ICollection<DemandVisibilityMember> VisibilityMembers { get; set; } = new List<DemandVisibilityMember>();
}
