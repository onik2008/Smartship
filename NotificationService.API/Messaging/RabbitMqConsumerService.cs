using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartShip.Shared.Constants;
using SmartShip.Shared.Models;

namespace NotificationService.API.Messaging;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private IConnection? _connection;
    private IChannel? _otpChannel;
    private IChannel? _emailChannel;

    public RabbitMqConsumerService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<RabbitMqConsumerService> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ConnectWithRetryAsync(stoppingToken);

        stoppingToken.Register(() =>
        {
            _logger.LogInformation("RabbitMQ consumer stopping.");
        });

        await Task.Delay(Timeout.Infinite, stoppingToken).ContinueWith(_ => { });
    }

    private async Task ConnectWithRetryAsync(CancellationToken stoppingToken)
    {
        int retryCount = 0;
        int maxRetries = 5;

        while (!stoppingToken.IsCancellationRequested && retryCount < maxRetries)
        {
            try
            {
                await InitializeRabbitMqAsync(stoppingToken);
                _logger.LogInformation("RabbitMQ consumer connected and listening.");
                return;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex,
                    "Failed to connect to RabbitMQ (attempt {Attempt}/{Max}). Retrying in 5 seconds...",
                    retryCount, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ContinueWith(_ => { });
            }
        }

        _logger.LogError("Could not connect to RabbitMQ after {Max} attempts. Notification consumer will not run.", maxRetries);
    }

    private async Task InitializeRabbitMqAsync(CancellationToken stoppingToken)
    {
        var rabbitConfig = _configuration.GetSection("RabbitMQ");

        var factory = new ConnectionFactory
        {
            HostName = rabbitConfig["Host"] ?? "localhost",
            Port = int.TryParse(rabbitConfig["Port"], out var port) ? port : 5672,
            UserName = rabbitConfig["Username"] ?? "guest",
            Password = rabbitConfig["Password"] ?? "guest",
            VirtualHost = rabbitConfig["VirtualHost"] ?? "/"
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _otpChannel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
        _emailChannel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _otpChannel.QueueDeclareAsync(
            queue: QueueNames.OtpQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _emailChannel.QueueDeclareAsync(
            queue: QueueNames.EmailQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await _otpChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);
        await _emailChannel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var otpConsumer = new AsyncEventingBasicConsumer(_otpChannel);
        otpConsumer.ReceivedAsync += OnOtpMessageReceivedAsync;
        await _otpChannel.BasicConsumeAsync(
            queue: QueueNames.OtpQueue,
            autoAck: false,
            consumer: otpConsumer,
            cancellationToken: stoppingToken);

        var emailConsumer = new AsyncEventingBasicConsumer(_emailChannel);
        emailConsumer.ReceivedAsync += OnEmailMessageReceivedAsync;
        await _emailChannel.BasicConsumeAsync(
            queue: QueueNames.EmailQueue,
            autoAck: false,
            consumer: emailConsumer,
            cancellationToken: stoppingToken);
    }

    private async Task OnOtpMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        _logger.LogInformation("Received OTP request message.");

        try
        {
            var message = JsonSerializer.Deserialize<OtpRequestMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<Services.INotificationService>();
                await notificationService.ProcessOtpRequestAsync(message);
            }

            if (_otpChannel != null)
                await _otpChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OTP message.");
            if (_otpChannel != null)
                await _otpChannel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task OnEmailMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        _logger.LogInformation("Received email request message.");

        try
        {
            var message = JsonSerializer.Deserialize<EmailRequestMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<Services.INotificationService>();
                await notificationService.ProcessEmailRequestAsync(message);
            }

            if (_emailChannel != null)
                await _emailChannel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing email message.");
            if (_emailChannel != null)
                await _emailChannel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);

        if (_otpChannel != null)
            await _otpChannel.CloseAsync(stoppingToken);
        if (_emailChannel != null)
            await _emailChannel.CloseAsync(stoppingToken);
        if (_connection != null)
            await _connection.CloseAsync(stoppingToken);
    }
}
