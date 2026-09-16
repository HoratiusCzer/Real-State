using REAK.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.API.Models.Entities;

public class Lead
{
    [Key]
    public int Id { get; set; }

    [Required]
    [ForeignKey(nameof(Client))]
    public int ClientId { get; set; }

    [ForeignKey(nameof(Property))]
    public int? PropertyId { get; set; }

    [Required]
    public LeadStatus Status { get; set; } = LeadStatus.New;

    [Required]
    public LeadSource Source { get; set; }

    [ForeignKey(nameof(AssignedAgent))]
    public int? AssignedAgentId { get; set; }

    public DateTime? FollowUpDate { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Budget { get; set; }

    [MaxLength(500)]
    public string? Requirements { get; set; }

    public int Priority { get; set; } = 1;

    // Navigation properties
    public virtual Client Client { get; set; } = null!;
    public virtual Property? Property { get; set; }
    public virtual User? AssignedAgent { get; set; }
}
