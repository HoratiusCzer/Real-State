using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Deal
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Property))]
    public int PropertyId { get; set; }

    [Required]
    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }

    [Required]
    [ForeignKey(nameof(Agent))]
    public int AgentId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal CommissionRate { get; set; }

    [Required]
    public DealStatus Status { get; set; } = DealStatus.Draft;

    public DateTime DealDate { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? ClosedDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    [MaxLength(255)]
    public string? ContractNumber { get; set; }

    // Navigation properties
    public virtual Property Property { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    public virtual User Agent { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();
}
