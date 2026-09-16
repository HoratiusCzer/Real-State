using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Admin-configurable property type (e.g. Residential, Commercial, Land). Referenced by
/// listings, demands, and matching criteria.</summary>
public class PropertyType
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<PropertySubtype> Subtypes { get; set; } = new List<PropertySubtype>();
}
