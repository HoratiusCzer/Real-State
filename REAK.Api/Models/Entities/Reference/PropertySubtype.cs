using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

public class PropertySubtype
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(PropertyType))]
    public Guid PropertyTypeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual PropertyType PropertyType { get; set; } = null!;
}
