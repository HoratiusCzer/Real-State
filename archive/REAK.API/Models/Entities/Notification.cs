using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public int UserId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public NotificationType Type { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    [MaxLength(100)]
    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
