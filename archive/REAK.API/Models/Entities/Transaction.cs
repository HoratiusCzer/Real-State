using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Deal))]
    public int DealId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(255)]
    public string? ReferenceNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Deal Deal { get; set; } = null!;
}
