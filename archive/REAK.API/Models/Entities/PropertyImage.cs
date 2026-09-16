using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class PropertyImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Property))]
    public int PropertyId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? Caption { get; set; }

    // Navigation properties
    public virtual Property Property { get; set; } = null!;
}
