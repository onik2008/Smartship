using System.Net;
using System.Text.Json;
using SmartShip.Shared.DTOs;
using SmartShip.Shared.Exceptions;
using AppException = SmartShip.Shared.Exceptions.ApplicationException;

namespace TrackingService.API.Middleware;

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
        var safePath = SanitizeForLog(context.Request.Path);
        var safeTraceId = SanitizeForLog(traceId);

        ApiErrorResponse errorResponse;

        switch (exception)
        {
            case ValidationException validationEx:
                _logger.LogWarning(exception,
                    "Validation error. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    validationEx.StatusCode,
                    validationEx.Message,
                    validationEx.Code,
                    validationEx.Errors,
                    traceId);
                break;

            case UnauthorizedException unauthorizedEx:
                _logger.LogWarning(exception,
                    "Unauthorized access. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    unauthorizedEx.StatusCode,
                    unauthorizedEx.Message,
                    unauthorizedEx.Code,
                    traceId: traceId);
                break;

            case ForbiddenException forbiddenEx:
                _logger.LogWarning(exception,
                    "Forbidden access. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    forbiddenEx.StatusCode,
                    forbiddenEx.Message,
                    forbiddenEx.Code,
                    traceId: traceId);
                break;

            case NotFoundException notFoundEx:
                _logger.LogInformation(exception,
                    "Resource not found. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    notFoundEx.StatusCode,
                    notFoundEx.Message,
                    notFoundEx.Code,
                    traceId: traceId);
                break;

            case ConflictException conflictEx:
                _logger.LogWarning(exception,
                    "Conflict error. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    conflictEx.StatusCode,
                    conflictEx.Message,
                    conflictEx.Code,
                    traceId: traceId);
                break;

            case BusinessException businessEx:
                _logger.LogWarning(exception,
                    "Business rule violation. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    businessEx.StatusCode,
                    businessEx.Message,
                    businessEx.Code,
                    traceId: traceId);
                break;

            case ServiceUnavailableException serviceEx:
                _logger.LogError(exception,
                    "Service unavailable. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    serviceEx.StatusCode,
                    serviceEx.Message,
                    serviceEx.Code,
                    traceId: traceId);
                break;

            case ExternalServiceException externalEx:
                _logger.LogError(exception,
                    "External service error: {ServiceName}. TraceId: {TraceId}, Path: {Path}",
                    externalEx.ServiceName, safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    externalEx.StatusCode,
                    externalEx.Message,
                    externalEx.Code,
                    traceId: traceId);
                break;

            case DataAccessException dataEx:
                _logger.LogError(exception,
                    "Data access error. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    dataEx.StatusCode,
                    "A data error occurred. Please try again later.",
                    dataEx.Code,
                    traceId: traceId);
                break;

            case MessagingException messagingEx:
                _logger.LogError(exception,
                    "Messaging error. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    messagingEx.StatusCode,
                    "A messaging error occurred. Please try again later.",
                    messagingEx.Code,
                    traceId: traceId);
                break;

            case AppException appEx:
                _logger.LogError(exception,
                    "Application error. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    appEx.StatusCode,
                    appEx.Message,
                    appEx.Code,
                    traceId: traceId);
                break;

            case ArgumentNullException argNullEx:
                _logger.LogError(exception,
                    "Null argument error: {ParamName}. TraceId: {TraceId}",
                    argNullEx.ParamName, safeTraceId);
                errorResponse = ApiErrorResponse.Create(
                    (int)HttpStatusCode.BadRequest,
                    "A required value was missing.",
                    "NULL_ARGUMENT",
                    traceId: traceId);
                break;

            case ArgumentException argEx:
                _logger.LogError(exception,
                    "Argument error: {ParamName}. TraceId: {TraceId}",
                    argEx.ParamName, safeTraceId);
                errorResponse = ApiErrorResponse.Create(
                    (int)HttpStatusCode.BadRequest,
                    "An invalid argument was provided.",
                    "INVALID_ARGUMENT",
                    traceId: traceId);
                break;

            case TimeoutException:
                _logger.LogError(exception,
                    "Request timed out. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    (int)HttpStatusCode.RequestTimeout,
                    "The request timed out. Please try again.",
                    "REQUEST_TIMEOUT",
                    traceId: traceId);
                break;

            case OperationCanceledException:
                _logger.LogWarning(
                    "Request was cancelled. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    (int)HttpStatusCode.BadRequest,
                    "The request was cancelled.",
                    "REQUEST_CANCELLED",
                    traceId: traceId);
                break;

            default:
                _logger.LogError(exception,
                    "Unhandled exception. TraceId: {TraceId}, Path: {Path}",
                    safeTraceId, safePath);
                errorResponse = ApiErrorResponse.Create(
                    (int)HttpStatusCode.InternalServerError,
                    "An unexpected error occurred. Please contact support.",
                    "INTERNAL_SERVER_ERROR",
                    new[] { "Please contact support if the problem persists." },
                    traceId);
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }

    private static string SanitizeForLog(string? value) =>
        value?.Replace("\r", "_", StringComparison.Ordinal)
              .Replace("\n", "_", StringComparison.Ordinal) ?? string.Empty;
}
