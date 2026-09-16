using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Task
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(AssignedToUser))]
    public int AssignedToUserId { get; set; }

    [Required]
    [ForeignKey(nameof(CreatedByUser))]
    public int CreatedByUserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required]
    public Enums.TaskPriority Priority { get; set; } = Enums.TaskPriority.Medium;

    [Required]
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    // Navigation properties
    public virtual User AssignedToUser { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
}
