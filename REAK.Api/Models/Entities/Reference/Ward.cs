using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

public class Ward
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Municipality))]
    public Guid MunicipalityId { get; set; }

    [Required]
    public int Number { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Municipality Municipality { get; set; } = null!;
    public virtual ICollection<Locality> Localities { get; set; } = new List<Locality>();
}
