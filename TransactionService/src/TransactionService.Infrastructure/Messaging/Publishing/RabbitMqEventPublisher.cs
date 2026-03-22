using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Options;
using TransactionService.Infrastructure.Messaging.Routing;
namespace TransactionService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _connectionOptions;
    private readonly TransactionEventsPublisherOptions _publisherOptions;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> connectionOptions,
        IOptions<TransactionEventsPublisherOptions> publisherOptions)
    {
        _connectionOptions = connectionOptions.Value;
        _publisherOptions = publisherOptions.Value;
        

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

    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var routingKey = IntegrationEventMap.GetName(message!.GetType());


        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _publisherOptions.Exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

    }

}
