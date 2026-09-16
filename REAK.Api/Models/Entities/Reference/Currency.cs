using System.ComponentModel.DataAnnotations;

namespace REAK.Api.Models.Entities.Reference;

public class Currency
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Symbol { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
