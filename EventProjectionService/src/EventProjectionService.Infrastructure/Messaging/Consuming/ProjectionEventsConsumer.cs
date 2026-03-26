using EventProjectionService.Infrastructure.Messaging.Options;
using EventProjectionService.Infrastructure.Persistence.Mongo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace EventProjectionService.Infrastructure.Messaging.Consuming;

public class ProjectionEventsConsumer : BackgroundService
{
    private readonly ILogger<ProjectionEventsConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqOptions> _connectionOptions;

    private IConnection? _connection;
    private IModel? _channel;

    public ProjectionEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> connectionOptions,
        ILogger<ProjectionEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _connectionOptions = connectionOptions;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _connectionOptions.Host,
            UserName = _connectionOptions.Username,
            Password = _connectionOptions.Password
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("auth.events", ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare("account.events", ExchangeType.Topic, durable: true);
        _channel.ExchangeDeclare("transaction.events", ExchangeType.Topic, durable: true);

        _channel.QueueDeclare(
            queue: "projection.events",
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind("projection.events", "auth.events", "#");
        _channel.QueueBind("projection.events", "account.events", "#");
        _channel.QueueBind("projection.events", "transaction.events", "#");

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
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
                _logger.LogError(ex, "Projection consumer failed");

                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: "projection.events",
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        base.Dispose();
    }
}