using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AccountService.Infrastructure.Messaging.Consuming;

public class TransactionEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TransactionEventsConsumerOptions _options;
    private readonly ILogger<TransactionEventsConsumer> _logger;
    private readonly RabbitMqOptions _rabbitMqOptions;


    private IConnection? _connection;
    private IModel? _channel;

    public TransactionEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<TransactionEventsConsumerOptions> options,
        ILogger<TransactionEventsConsumer> logger,
        IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _rabbitMqOptions.Host,
                    UserName = _rabbitMqOptions.Username,
                    Password = _rabbitMqOptions.Password,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.BasicQos(0, 1, false);

                _logger.LogInformation("RabbitMQ connection established");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RabbitMQ connection failed. Retrying...");
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (_channel is null)
            return;

        _channel.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: "transfer.*");

        _channel.QueueBind(
            queue: _options.Queue,
            exchange: _options.Exchange,
            routingKey: "withdrawal.*");

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                _logger.LogInformation(
                    "RabbitMQ message received. RoutingKey: {RoutingKey}, Body: {Body}",
                    ea.RoutingKey,
                    json);

                using var scope = _scopeFactory.CreateScope();

                var inboxWriter = scope.ServiceProvider
                    .GetRequiredService<IInboxWriter>();

                if (string.IsNullOrEmpty(ea.BasicProperties.MessageId))
                {
                    _logger.LogWarning("Message received without MessageId");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (!Guid.TryParse(ea.BasicProperties.MessageId, out var messageId))
                {
                    _logger.LogWarning("Invalid MessageId format: {MessageId}", ea.BasicProperties.MessageId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var eventType = ea.RoutingKey;

                await inboxWriter.SaveAsync(
                    messageId,
                    eventType, 
                    json, 
                    stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);

                _logger.LogInformation("Message stored in Inbox: {MessageId}", messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message");

                _channel.BasicNack(
                    ea.DeliveryTag,
                    false,
                    true);
            }
        };

        _channel.BasicConsume(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
