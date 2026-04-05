using System.Net;
using System.Text.Json;

namespace SmartShip.Gateway.Middleware;

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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        int statusCode;
        string message;
        string code;

        switch (exception)
        {
            case TimeoutException:
                _logger.LogError(exception,
                    "Gateway request timed out. TraceId: {TraceId}, Path: {Path}",
                    traceId, context.Request.Path);
                statusCode = (int)HttpStatusCode.GatewayTimeout;
                message = "The request timed out. Please try again.";
                code = "GATEWAY_TIMEOUT";
                break;

            case OperationCanceledException:
                _logger.LogWarning(
                    "Gateway request cancelled. TraceId: {TraceId}, Path: {Path}",
                    traceId, context.Request.Path);
                statusCode = 499;
                message = "The request was cancelled.";
                code = "REQUEST_CANCELLED";
                break;

            default:
                _logger.LogError(exception,
                    "Unhandled gateway exception. TraceId: {TraceId}, Path: {Path}",
                    traceId, context.Request.Path);
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred at the gateway.";
                code = "GATEWAY_ERROR";
                break;
        }

        var errorResponse = new
        {
            success = false,
            statusCode,
            message,
            code,
            details = new[] { "Please contact support if the problem persists." },
            timestamp = DateTime.UtcNow,
            traceId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}
