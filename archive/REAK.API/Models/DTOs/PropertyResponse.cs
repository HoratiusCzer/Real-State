using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class PropertyResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PropertyType Type { get; set; }
    public PropertyStatus Status { get; set; }
    public decimal Price { get; set; }
    public decimal Area { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? Bedrooms { get; set; }
    public string? Bathrooms { get; set; }
    public int? YearBuilt { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PropertyImageResponse> Images { get; set; } = new();
}
