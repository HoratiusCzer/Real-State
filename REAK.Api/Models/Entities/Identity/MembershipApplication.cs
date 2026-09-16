using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using REAK.Api.Models.Enums;

namespace REAK.Api.Models.Entities.Identity;

/// <summary>Flow A step 1-2: a visitor submits this via the public site; an admin reviews and approves,
/// which is what leads to an Invitation being created (spec §2.4).</summary>
public class MembershipApplication
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string ContactName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Message { get; set; }

    [Required]
    public MembershipApplicationStatus Status { get; set; } = MembershipApplicationStatus.Pending;

    [ForeignKey(nameof(ReviewedByProfile))]
    public Guid? ReviewedByProfileId { get; set; }

    public DateTime? ReviewedAt { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Profile? ReviewedByProfile { get; set; }
}
