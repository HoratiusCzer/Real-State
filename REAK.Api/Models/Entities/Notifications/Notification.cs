using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Notifications;

/// <summary>In-app notification (spec §14). Supports unread/read state and mark-all-read.</summary>
public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Body { get; set; }

    [MaxLength(500)]
    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile Profile { get; set; } = null!;
}
