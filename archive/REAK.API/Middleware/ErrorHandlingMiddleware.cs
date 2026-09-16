using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace REAK.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = Guid.NewGuid().ToString();
        var statusCode = GetStatusCode(exception);
        var errorResponse = CreateErrorResponse(exception, errorId, statusCode);

        LogError(exception, errorId, context);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }

    private HttpStatusCode GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ForbiddenException => HttpStatusCode.Forbidden,
            NotFoundException => HttpStatusCode.NotFound,
            ConflictException => HttpStatusCode.Conflict,
            BadRequestException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };
    }

    private ErrorResponse CreateErrorResponse(Exception exception, string errorId, HttpStatusCode statusCode)
    {
        var response = new ErrorResponse
        {
            ErrorId = errorId,
            StatusCode = (int)statusCode,
            Message = GetUserFriendlyMessage(exception),
            Timestamp = DateTime.UtcNow
        };

        if (exception is ValidationException validationException)
        {
            response.Errors = validationException.Errors;
        }

        if (_environment.IsDevelopment())
        {
            response.Details = exception.Message;
            response.StackTrace = exception.StackTrace;
        }

        return response;
    }

    private string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            ValidationException => "One or more validation errors occurred.",
            UnauthorizedAccessException => "You are not authorized to access this resource.",
            ForbiddenException => "You do not have permission to perform this action.",
            NotFoundException notFound => notFound.Message,
            ConflictException conflict => conflict.Message,
            BadRequestException badRequest => badRequest.Message,
            _ => "An unexpected error occurred. Please try again later."
        };
    }

    private void LogError(Exception exception, string errorId, HttpContext context)
    {
        var logLevel = exception switch
        {
            ValidationException or BadRequestException => LogLevel.Warning,
            UnauthorizedAccessException or ForbiddenException => LogLevel.Warning,
            NotFoundException => LogLevel.Information,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "Error {ErrorId} occurred while processing request {Method} {Path}. User: {User}",
            errorId,
            context.Request.Method,
            context.Request.Path,
            context.User?.Identity?.Name ?? "Anonymous");
    }
}

public class ErrorResponse
{
    public string ErrorId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
}
