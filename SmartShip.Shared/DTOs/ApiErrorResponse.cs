namespace SmartShip.Shared.DTOs;

/// <summary>Standardized error response returned for all failed API requests.</summary>
public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public static ApiErrorResponse Create(
        int statusCode,
        string message,
        string code,
        IEnumerable<string>? details = null,
        string? traceId = null)
    {
        return new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Code = code,
            Details = details?.ToList() ?? new List<string>(),
            Timestamp = DateTime.UtcNow,
            TraceId = traceId
        };
    }
}
