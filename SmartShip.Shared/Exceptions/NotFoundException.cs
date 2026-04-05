namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a requested resource cannot be found (HTTP 404).</summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(string message, string code = "NOT_FOUND")
        : base(message, code, 404) { }

    public NotFoundException(string resourceName, object resourceKey, string code = "NOT_FOUND")
        : base($"{resourceName} with key '{resourceKey}' was not found.", code, 404) { }
}
