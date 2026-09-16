using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class PropertySearchRequest
{
    public string? SearchTerm { get; set; }
    public PropertyType? Type { get; set; }
    public PropertyStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public string? Location { get; set; }
    public int? BranchId { get; set; }
    public string? Bedrooms { get; set; }
    public string? Bathrooms { get; set; }
    public bool? IsFeatured { get; set; }

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }

    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
