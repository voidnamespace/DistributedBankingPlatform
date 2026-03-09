using RabbitMQ.Client;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
namespace TransactionService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;


    private const string ExchangeName = "trans.events";

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
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

    }

    public Task PublishAsync<T>(T message, string routingKey, CancellationToken ct = default)
    {
        
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

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

        return Task.CompletedTask;
    }


    public Task PublishRawAsync(string payloadJson, string routingKey, CancellationToken ct)
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

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

    }

}
