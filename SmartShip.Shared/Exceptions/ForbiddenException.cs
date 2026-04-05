namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when an authenticated user lacks permission for the requested action (HTTP 403).</summary>
public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden", string code = "FORBIDDEN")
        : base(message, code, 403) { }

    public ForbiddenException(string message, string code, Exception innerException)
        : base(message, code, 403, innerException) { }
}
