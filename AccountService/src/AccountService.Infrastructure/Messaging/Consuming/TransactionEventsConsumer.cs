using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging.Options;
using AccountService.Infrastructure.Persistence.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace AccountService.Infrastructure.Messaging.Consuming;

public class TransactionEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AuthEventsConsumerOptions _options;

    private IConnection? _connection;
    private IModel? _channel;


    public TransactionEventsConsumer(
    IServiceScopeFactory scopeFactory,
    IOptions<AuthEventsConsumerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    UserName = _options.Username,
                    Password = _options.Password,
                    DispatchConsumersAsync = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _channel.BasicQos(0, 1, false);
                break;
            }
            catch
            {
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
            routingKey: "user.*");


        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                        .GetRequiredService<AccountDbContext>();

                if (string.IsNullOrEmpty(ea.BasicProperties.MessageId))
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var messageId = Guid.Parse(ea.BasicProperties.MessageId);



                var inbox = new InboxMessage
                {
                    Id = messageId,
                    Payload = json,
                    RoutingKey = ea.RoutingKey,
                    Processed = false,
                    AttemptCount = 0,
                    ReceivedAt = DateTime.UtcNow
                };

                db.InboxMessages.Add(inbox);
                await db.SaveChangesAsync();

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
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
