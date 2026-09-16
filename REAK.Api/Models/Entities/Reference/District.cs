using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

public class District
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [ForeignKey(nameof(Province))]
    public Guid ProvinceId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Province Province { get; set; } = null!;
    public virtual ICollection<Municipality> Municipalities { get; set; } = new List<Municipality>();
}
