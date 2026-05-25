using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using UserSegmentationService.Application.Interfaces.Messaging;
using UserSegmentationService.Infrastructure.Outbox;

namespace UserSegmentationService.Infrastructure.Messaging.Publishers;

internal class EventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<EventPublisher> _logger;
    private readonly string _exchange;

    public EventPublisher(
        IConfiguration configuration,
        ILogger<EventPublisher> logger)
    {
        _logger = logger;
        _exchange = configuration["SegmentationEventsPublisher:Exchange"] ?? "segmentation.events";

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:Username"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true);
    }

    public Task PublishAsync(
        object message,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var routingKey = IntegrationEventTypeMap.GetName(message.GetType());
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.MessageId = Guid.NewGuid().ToString();
        properties.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "RabbitMQ event published. Type={EventType}, RoutingKey={RoutingKey}, Exchange={Exchange}",
            message.GetType().Name,
            routingKey,
            _exchange);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}
