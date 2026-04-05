namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a required service is temporarily unavailable (HTTP 503).</summary>
public class ServiceUnavailableException : ApplicationException
{
    public ServiceUnavailableException(string message = "Service is temporarily unavailable", string code = "SERVICE_UNAVAILABLE")
        : base(message, code, 503) { }

    public ServiceUnavailableException(string message, string code, Exception innerException)
        : base(message, code, 503, innerException) { }
}
