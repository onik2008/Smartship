namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a business rule or logic violation occurs (HTTP 422).</summary>
public class BusinessException : ApplicationException
{
    public BusinessException(string message, string code = "BUSINESS_ERROR")
        : base(message, code, 422) { }

    public BusinessException(string message, string code, Exception innerException)
        : base(message, code, 422, innerException) { }
}
