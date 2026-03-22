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
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;


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
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true);

    }

    public Task PublishAsync<T>(T message, CancellationToken ct = default)
    {
        if (message == null)
            throw new ArgumentNullException(nameof(message));

        var routingKey = RoutingKeyMap.Get(message.GetType());


        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.MessageId = Guid.NewGuid().ToString();
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: _options.Exchange,
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
