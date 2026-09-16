namespace REAK.API.Models.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; }

    public ApiResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ApiResponse<T> FailureResult(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            Timestamp = DateTime.UtcNow
        };
    }
}
