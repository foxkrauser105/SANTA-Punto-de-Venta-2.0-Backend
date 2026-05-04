using SANTA.PoS.Domain.Exceptions;

namespace SANTA.PoS.Middleware;

/// <summary>
/// Global exception handling middleware - catches all exceptions and returns appropriate HTTP responses
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case DomainException domainEx:
                // Business logic errors
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.Message = domainEx.Message;
                response.ErrorCode = "DOMAIN_ERROR";
                break;

            case ArgumentNullException argNullEx:
                // Missing required parameters
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = $"Required parameter is null: {argNullEx.ParamName}";
                response.ErrorCode = "NULL_ARGUMENT";
                break;

            case ArgumentException argEx:
                // Invalid parameters
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = argEx.Message;
                response.ErrorCode = "INVALID_ARGUMENT";
                break;

            default:
                // Unexpected errors
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = "An unexpected error occurred";
                response.ErrorCode = "INTERNAL_ERROR";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
