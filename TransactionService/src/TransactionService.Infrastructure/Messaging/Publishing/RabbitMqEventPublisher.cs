using RabbitMQ.Client;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Options;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Metadata;
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

    }


}
