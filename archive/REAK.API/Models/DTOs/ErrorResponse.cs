namespace REAK.API.Models.DTOs;

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public List<string>? Errors { get; set; }
    public string? StackTrace { get; set; }
    public DateTime Timestamp { get; set; }

    public ErrorResponse()
    {
        Timestamp = DateTime.UtcNow;
    }

    public ErrorResponse(string message, string? errorCode = null)
    {
        Message = message;
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }

    public ErrorResponse(string message, List<string> errors, string? errorCode = null)
    {
        Message = message;
        Errors = errors;
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }

    public static ErrorResponse Create(string message, string? errorCode = null)
    {
        return new ErrorResponse
        {
            Message = message,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };
    }

    public static ErrorResponse CreateWithErrors(string message, List<string> errors, string? errorCode = null)
    {
        return new ErrorResponse
        {
            Message = message,
            Errors = errors,
            ErrorCode = errorCode,
            Timestamp = DateTime.UtcNow
        };
    }
}
