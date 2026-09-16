using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Entities.Identity;

namespace REAK.Api.Models.Entities.Audit;

/// <summary>Append-only audit trail (spec §19). Enforced at the database level via a trigger that
/// blocks UPDATE/DELETE (see Data/Security/AuditLogAppendOnly.sql) — not just an app-level convention.
/// Summary/Metadata must stay short and never duplicate full sensitive records or contact info.</summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; }

    [ForeignKey(nameof(ActorProfile))]
    public Guid? ActorProfileId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    [MaxLength(500)]
    public string? Summary { get; set; }

    [MaxLength(2000)]
    public string? MetadataJson { get; set; }

    [MaxLength(64)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile? ActorProfile { get; set; }
}
