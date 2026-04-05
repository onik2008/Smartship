namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when an external third-party service call fails (HTTP 502).</summary>
public class ExternalServiceException : ApplicationException
{
    public string ServiceName { get; }

    public ExternalServiceException(string serviceName, string message, string code = "EXTERNAL_SERVICE_ERROR")
        : base(message, code, 502)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, string code, Exception innerException)
        : base(message, code, 502, innerException)
    {
        ServiceName = serviceName;
    }
}
