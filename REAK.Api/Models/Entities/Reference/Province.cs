using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Top level of the Nepal location hierarchy: Province -> District -> Municipality -> Ward -> Locality (spec §10).
/// Admin can add, edit, disable, and reorder entries at every level.</summary>
public class Province
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}
