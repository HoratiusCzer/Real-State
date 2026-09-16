using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

public class Municipality
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(District))]
    public Guid DistrictId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual District District { get; set; } = null!;
    public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
