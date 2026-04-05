namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a request lacks valid authentication credentials (HTTP 401).</summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access", string code = "UNAUTHORIZED")
        : base(message, code, 401) { }

    public UnauthorizedException(string message, string code, Exception innerException)
        : base(message, code, 401, innerException) { }
}
