using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SmartShip.Shared.Models;

namespace IdentityService.API.Messaging;

public interface IRabbitMqPublisher
{
    Task PublishOtpRequestAsync(OtpRequestMessage message);
    Task InitializeAsync();
}

public class RabbitMqPublisher : IRabbitMqPublisher, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _initialized;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var rabbitConfig = _configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = rabbitConfig["Host"] ?? "localhost",
            Port = int.TryParse(rabbitConfig["Port"], out var port) ? port : 5672,
            UserName = rabbitConfig["Username"] ?? "guest",
            Password = rabbitConfig["Password"] ?? "guest",
            VirtualHost = rabbitConfig["VirtualHost"] ?? "/"
        };

        try
        {
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.QueueDeclareAsync(
                queue: SmartShip.Shared.Constants.QueueNames.OtpQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _initialized = true;
            _logger.LogInformation("RabbitMQ publisher initialized.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ. OTP messages will not be published.");
        }
    }

    public async Task PublishOtpRequestAsync(OtpRequestMessage message)
    {
        if (!_initialized)
            await InitializeAsync();

        if (_channel == null)
        {
            _logger.LogWarning("RabbitMQ channel is not available. Skipping OTP publish.");
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

            _logger.LogInformation("OTP request published successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish OTP request.");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_connection != null)
            await _connection.CloseAsync();
    }
}
