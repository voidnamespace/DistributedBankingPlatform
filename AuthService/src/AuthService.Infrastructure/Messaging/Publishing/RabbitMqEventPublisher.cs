using System.Text;
using System.Text.Json;
using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "auth.events";

    public RabbitMqEventPublisher(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.User,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        _channel.BasicReturn += (sender, args) =>
        {
            _logger.LogWarning(
                "RabbitMQ message returned. RoutingKey={RoutingKey}",
                args.RoutingKey);
        };

        _logger.LogInformation("RabbitMQ publisher initialized");
    }

    public Task PublishAsync<T>(
        T message,
        string routingKey,
        CancellationToken ct = default)
    {
        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        _logger.LogInformation(
            "RabbitMQ event published. Type={EventType} RoutingKey={RoutingKey}",
            typeof(T).Name,
            routingKey);

        return Task.CompletedTask;
    }

    public Task PublishRawAsync(
        string payloadJson,
        string routingKey,
        CancellationToken ct)
    {
        var body = Encoding.UTF8.GetBytes(payloadJson);

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body);

        _logger.LogInformation(
            "RabbitMQ RAW message published. RoutingKey={RoutingKey}",
            routingKey);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ publisher disposed");
    }
}