using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Leaf level of the Nepal location hierarchy — a neighborhood/area within a ward.</summary>
public class Locality
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Ward))]
    public Guid WardId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Ward Ward { get; set; } = null!;
}
