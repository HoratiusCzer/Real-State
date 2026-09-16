using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Branch
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [ForeignKey(nameof(Manager))]
    public int? ManagerId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User? Manager { get; set; }
    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
