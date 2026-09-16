using REAK.API.Models.Enums;

namespace REAK.API.Models.DTOs;

public class ClientSearchRequest
{
    public string? SearchTerm { get; set; }
    public LeadSource? Source { get; set; }
    public int? MinLeadScore { get; set; }
    public int? MaxLeadScore { get; set; }

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
