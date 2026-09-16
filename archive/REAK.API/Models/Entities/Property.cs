using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Property
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public PropertyType Type { get; set; }

    [Required]
    public PropertyStatus Status { get; set; } = PropertyStatus.Available;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Area { get; set; }

    [Required]
    [MaxLength(500)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Address { get; set; }

    [ForeignKey(nameof(Branch))]
    public int BranchId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(10)]
    public string? Bedrooms { get; set; }

    [MaxLength(10)]
    public string? Bathrooms { get; set; }

    public int? YearBuilt { get; set; }

    [MaxLength(50)]
    public string? Latitude { get; set; }

    [MaxLength(50)]
    public string? Longitude { get; set; }

    public bool IsFeatured { get; set; } = false;

    // Navigation properties
    public virtual Branch Branch { get; set; } = null!;
    public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
}
