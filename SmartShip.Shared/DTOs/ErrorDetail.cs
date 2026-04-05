namespace SmartShip.Shared.DTOs;

/// <summary>Represents a single validation or error detail item.</summary>
public class ErrorDetail
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public ErrorDetail() { }

    public ErrorDetail(string field, string message)
    {
        Field = field;
        Message = message;
    }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Field) ? Message : $"{Field}: {Message}";
}
