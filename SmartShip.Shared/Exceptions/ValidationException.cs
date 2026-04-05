namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when input data fails validation (HTTP 400).</summary>
public class ValidationException : ApplicationException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string>? errors = null, string code = "VALIDATION_ERROR")
        : base(message, code, 400)
    {
        Errors = errors?.ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
    }

    public ValidationException(IEnumerable<string> errors, string code = "VALIDATION_ERROR")
        : base("Validation failed", code, 400)
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
