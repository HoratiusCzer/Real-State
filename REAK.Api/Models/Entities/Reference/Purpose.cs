using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Admin-configurable purpose (spec §9: buyer, tenant, investor, other), shared by listings
/// (e.g. "for sale"/"for rent") and demands.</summary>
public class Purpose
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
