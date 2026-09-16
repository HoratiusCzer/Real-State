namespace REAK.API.Models.DTOs;

public class PropertyImageResponse
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
}
