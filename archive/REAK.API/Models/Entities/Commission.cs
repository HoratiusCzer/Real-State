using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Commission
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Deal))]
    public int DealId { get; set; }

    [Required]
    [ForeignKey(nameof(Agent))]
    public int AgentId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal PercentageRate { get; set; }

    [Required]
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;

    public DateTime? PaidDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(255)]
    public string? PaymentReference { get; set; }

    // Navigation properties
    public virtual Deal Deal { get; set; } = null!;
    public virtual User Agent { get; set; } = null!;
}
