using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace REAK.Api.Models.Entities.Reference;

/// <summary>Land area units (spec §11): Ropani, Aana, Paisa, Dam, Bigha, Kattha, Dhur, Square Feet,
/// Square Metres. ConversionToSquareMeters stays null (conversion disabled) until an admin explicitly
/// configures and approves it — never invent conversion coefficients.</summary>
public class AreaUnit
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Abbreviation { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? ConversionToSquareMeters { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
