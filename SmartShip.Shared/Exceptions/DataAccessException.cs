namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a database or repository operation fails (HTTP 500).</summary>
public class DataAccessException : ApplicationException
{
    public DataAccessException(string message = "A data access error occurred", string code = "DATA_ACCESS_ERROR")
        : base(message, code, 500) { }

    public DataAccessException(string message, string code, Exception innerException)
        : base(message, code, 500, innerException) { }
}
