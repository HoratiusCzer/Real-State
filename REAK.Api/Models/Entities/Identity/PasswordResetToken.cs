using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>A single-use, time-limited token backing the forgot/reset password flow (spec §6).
/// Only the hash is stored. Issuing a new one does not need to invalidate a prior unused one
/// explicitly — redemption checks UsedAt/ExpiresAt, and any unused stale rows simply expire.</summary>
public class PasswordResetToken
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Profile))]
    public Guid ProfileId { get; set; }

    [Required]
    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public virtual Profile Profile { get; set; } = null!;
}
