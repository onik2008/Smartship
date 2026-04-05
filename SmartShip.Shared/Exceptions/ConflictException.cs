namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a request conflicts with the current state of a resource (HTTP 409).</summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message, string code = "CONFLICT")
        : base(message, code, 409) { }

    public ConflictException(string message, string code, Exception innerException)
        : base(message, code, 409, innerException) { }
}
