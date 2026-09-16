using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Demands;

/// <summary>Structurally isolated client identity/contact info (spec §2.3, §9.1). Never exposed by
/// normal demand queries or demand search; RLS-locked to the owning organization by default, disclosed
/// to another organization only via an explicit CollaborationContactDisclosure grant.</summary>
public class DemandContact
{
    [Key]
    [ForeignKey(nameof(Demand))]
    public Guid DemandId { get; set; }

    [MaxLength(255)]
    public string? ClientName { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(1000)]
    public string? ConfidentialNotes { get; set; }

    public virtual Demand Demand { get; set; } = null!;
}
