namespace SmartShip.Shared.Utils;

/// <summary>Utility methods for sanitizing values before logging to prevent log injection attacks.</summary>
public static class LogSanitizer
{
    /// <summary>
    /// Sanitizes a string value for safe inclusion in log messages by replacing
    /// newline characters that could be used for log injection (log forging) attacks.
    /// </summary>
    public static string Sanitize(string? value) =>
        value?.Replace("\r", "_", StringComparison.Ordinal)
              .Replace("\n", "_", StringComparison.Ordinal) ?? string.Empty;
}
