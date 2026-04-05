namespace SmartShip.Shared.Exceptions;

/// <summary>Thrown when a messaging (RabbitMQ) operation fails (HTTP 500).</summary>
public class MessagingException : ApplicationException
{
    public MessagingException(string message = "A messaging error occurred", string code = "MESSAGING_ERROR")
        : base(message, code, 500) { }

    public MessagingException(string message, string code, Exception innerException)
        : base(message, code, 500, innerException) { }
}
