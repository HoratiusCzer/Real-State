using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Cms;

/// <summary>Admin-configurable site navigation (spec §4.3 Admin CMS scope: "navigation").</summary>
public class NavigationItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Label { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [ForeignKey(nameof(Parent))]
    public Guid? ParentId { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual NavigationItem? Parent { get; set; }
    public virtual ICollection<NavigationItem> Children { get; set; } = new List<NavigationItem>();
}
