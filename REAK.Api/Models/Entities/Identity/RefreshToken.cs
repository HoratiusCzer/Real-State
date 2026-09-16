using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>A rotating refresh token backing "session handling" (spec §6). The access token (JWT)
/// is short-lived and stateless; this is the revocable half — logout, password reset, and
/// suspension all work by revoking rows here rather than needing a token blocklist. Only the
/// hash is stored, never the raw token.</summary>
public class RefreshToken
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

    [MaxLength(64)]
    public string? CreatedByIp { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    /// <summary>Set when this token was rotated out in favor of a newer one (reuse of a revoked
    /// token — e.g. a stolen, already-rotated refresh token being replayed — is a signal worth
    /// detecting later; recording the chain makes that possible).</summary>
    public Guid? ReplacedByTokenId { get; set; }

    public virtual Profile Profile { get; set; } = null!;
}
