using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Messaging.Options;
using AuthService.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _connectionOptions;
    private readonly AuthEventsPublisherOptions _publisherOptions;

    private readonly ILogger<RabbitMqEventPublisher> _logger;

    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(
        IOptions<RabbitMqOptions> connectionOptions,
        IOptions<AuthEventsPublisherOptions> publisherOptions,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connectionOptions = connectionOptions.Value;
        _publisherOptions = publisherOptions.Value;

        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _connectionOptions.Host,
            Port = _connectionOptions.Port,
            UserName = _connectionOptions.User,
            Password = _connectionOptions.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _publisherOptions.Exchange,
            type: ExchangeType.Topic,
            durable: true);

        _channel.BasicReturn += (sender, args) =>
        {
            _logger.LogWarning(
                "RabbitMQ message returned. RoutingKey={RoutingKey}",
                args.RoutingKey);
        };

        _logger.LogInformation(
            "RabbitMQ publisher initialized. Exchange={Exchange}",
            _publisherOptions.Exchange);
    }

    public Task PublishAsync<T>(
        T message,
        CancellationToken ct = default)
    {
        var routingKey = RoutingKeyMap.Get(typeof(T));

        var body = Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();

        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _publisherOptions.Exchange,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        _logger.LogInformation(
            "RabbitMQ event published. Type={EventType} RoutingKey={RoutingKey}",
            typeof(T).Name,
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