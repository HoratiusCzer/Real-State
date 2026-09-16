namespace REAK.API.Models.DTOs;

public class PaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Success";
    public List<T> Data { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
    public DateTime Timestamp { get; set; }

    public PaginatedResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    public PaginatedResponse(List<T> data, int currentPage, int pageSize, int totalCount)
    {
        Success = true;
        Message = "Success";
        Data = data;
        Pagination = new PaginationMetadata
        {
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
        Timestamp = DateTime.UtcNow;
    }

    public static PaginatedResponse<T> Create(List<T> data, int currentPage, int pageSize, int totalCount, string message = "Success")
    {
        return new PaginatedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = new PaginationMetadata
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            },
            Timestamp = DateTime.UtcNow
        };
    }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
