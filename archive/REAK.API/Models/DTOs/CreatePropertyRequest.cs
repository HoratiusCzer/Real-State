using System.ComponentModel.DataAnnotations;
using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class CreatePropertyRequest
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public PropertyType Type { get; set; }

    public PropertyStatus? Status { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
    public decimal Price { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Area must be a positive value")]
    public decimal Area { get; set; }

    [Required]
    [MaxLength(500)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Address { get; set; }

    [Required]
    public int BranchId { get; set; }

    [MaxLength(10)]
    public string? Bedrooms { get; set; }

    [MaxLength(10)]
    public string? Bathrooms { get; set; }

    public int? YearBuilt { get; set; }

    [MaxLength(50)]
    public string? Latitude { get; set; }

    [MaxLength(50)]
    public string? Longitude { get; set; }

    public bool IsFeatured { get; set; } = false;
}
