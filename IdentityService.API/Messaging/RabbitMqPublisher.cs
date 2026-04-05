using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SmartShip.Shared.Models;

namespace IdentityService.API.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishOtpRequestAsync(OtpRequestMessage message);
}

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
        var rabbitConfig = configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = rabbitConfig["Host"] ?? "localhost",
            Port = int.Parse(rabbitConfig["Port"] ?? "5672"),
            UserName = rabbitConfig["Username"] ?? "guest",
            Password = rabbitConfig["Password"] ?? "guest",
            VirtualHost = rabbitConfig["VirtualHost"] ?? "/"
        };

        try
        {
            _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

            _channel.QueueDeclareAsync(
                queue: SmartShip.Shared.Constants.QueueNames.OtpQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ. OTP messages will not be published.");
            _connection = null!;
            _channel = null!;
        }
    }

    public async Task PublishOtpRequestAsync(OtpRequestMessage message)
    {
        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is not available. Skipping OTP publish for {Email}.", message.Email);
            return;
        }

        try
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties { Persistent = true };

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: SmartShip.Shared.Constants.QueueNames.OtpQueue,
                mandatory: false,
                basicProperties: props,
                body: body);

            _logger.LogInformation("OTP request published for {Email}", message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish OTP request for {Email}", message.Email);
        }
    }

    public void Dispose()
    {
        _channel?.CloseAsync().GetAwaiter().GetResult();
        _connection?.CloseAsync().GetAwaiter().GetResult();
    }
}
