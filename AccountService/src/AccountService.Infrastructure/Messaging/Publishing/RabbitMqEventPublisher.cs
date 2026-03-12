using AccountService.Application.Interfaces.Messaging;
using AccountService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
namespace AccountService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{

    private readonly RabbitMqOptions _options;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    private const string ExchangeName = "account.transaction.events";

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


    public Task PublishAsync<T>(T message,
        string routingKey, CancellationToken ct)
    {

    }



}
