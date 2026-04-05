namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when an unexpected internal server error occurs (HTTP 500).</summary>
public class InternalServerException : ApplicationException
{
    public InternalServerException(string message = "An unexpected error occurred", string code = "INTERNAL_SERVER_ERROR")
        : base(message, code, 500) { }

    public InternalServerException(string message, string code, Exception innerException)
        : base(message, code, 500, innerException) { }
}
