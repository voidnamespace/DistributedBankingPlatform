using AuditLogService.Infrastructure.Messaging.Options;
using AuditLogService.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace AuditLogService.Infrastructure.Messaging.Consuming;

public class AuditLogEventsConsumer : BackgroundService
{
    private readonly ILogger<AuditLogEventsConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqOptions> _connectionOptions;
    private readonly IOptions<AuthEventsConsumerOptions> _authEventsConsumerOptions;
    private readonly IOptions<AuditLogEventsConsumerOptions> _queueOptions;

    private IConnection? _connection;
    private IModel? _channel;

    public AuditLogEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> connectionOptions,
        ILogger<AuditLogEventsConsumer> logger,
        IOptions<AuthEventsConsumerOptions> authEventsConsumerOptions,
        IOptions<AuditLogEventsConsumerOptions> queueOptions)
    {
        _scopeFactory = scopeFactory;
        _connectionOptions = connectionOptions;
        _logger = logger;
        _authEventsConsumerOptions = authEventsConsumerOptions;
        _queueOptions = queueOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _connectionOptions.Value.Host,
                    UserName = _connectionOptions.Value.Username,
                    Password = _connectionOptions.Value.Password
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
            exchange: _authEventsConsumerOptions.Value.Exchange,
            type: ExchangeType.Topic,
            durable: true);


            _channel.QueueDeclare(
                queue: _queueOptions.Value.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                queue: _queueOptions.Value.Queue,
                exchange: _authEventsConsumerOptions.Value.Exchange,
                routingKey: "user.*");


            var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            if (stoppingToken.IsCancellationRequested)
                return;

            try
            {
                _logger.LogInformation("EVENT RECEIVED {routingKey}", ea.RoutingKey);

                var body = ea.Body.ToArray();
                var payload = Encoding.UTF8.GetString(body);

                var storedEvent = new StoredEvent
                {
                    Id = Guid.NewGuid(),
                    Type = ea.RoutingKey,
                    Payload = payload,
                    ReceivedAt = DateTime.UtcNow
                };

                using var scope = _scopeFactory.CreateScope();

                var repository = scope
                    .ServiceProvider
                    .GetRequiredService<MongoEventRepository>();

                await repository.SaveAsync(storedEvent);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuditLog consumer failed");

                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
                queue: _queueOptions.Value.Queue,
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