namespace SmartShip.Shared.Exceptions;

/// <summary>Base exception for all application-specific exceptions.</summary>
public abstract class ApplicationException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    protected ApplicationException(string message, string code, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    protected ApplicationException(string message, string code, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}
