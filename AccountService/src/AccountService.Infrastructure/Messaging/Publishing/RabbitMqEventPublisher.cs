using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Messaging.Options;
using AccountService.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AccountService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{

    private readonly RabbitMqOptions _connectionOptions;
    private readonly AccountEventsPublisherOptions _publisherOptions;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> connectionOptions,
        IOptions<AccountEventsPublisherOptions> publisherOptions,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connectionOptions = connectionOptions.Value;
        _publisherOptions = publisherOptions.Value;
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _connectionOptions.Host,
            Port = _connectionOptions.Port,
            UserName = _connectionOptions.Username,
            Password = _connectionOptions.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: _publisherOptions.Exchange,
            type: ExchangeType.Topic,
            durable: true);
    }


    public Task PublishAsync<T>(
       T message,
       CancellationToken ct = default)
    {
        var routingKey = IntegrationEventTypeMap.GetName(message!.GetType());

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
