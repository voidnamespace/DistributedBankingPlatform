using AccountService.Application.IntegrationEvents;
using AccountService.Infrastructure.Data;
using AccountService.Infrastructure.Messaging.Options;
using AccountService.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace AccountService.Infrastructure.Messaging.Consuming;

public class AccountEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConsumerOptions _options;
    private readonly ILogger<AccountEventsConsumer> _logger;

    private IConnection? _connection;
    private IModel? _channel;

    private static readonly Dictionary<string, Type> EventTypes = new()
    {
        { "UserCreatedIntegrationEvent", typeof(UserCreatedIntegrationEvent) },
        { "UserDeletedIntegrationEvent", typeof(UserDeletedIntegrationEvent) }
    };

    public AccountEventsConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<ConsumerOptions> options,
        ILogger<AccountEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
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
                _logger.LogInformation("RabbitMQ connected");
                break;
            }
            catch
            {
                _logger.LogInformation("Waiting RabbitMQ...");
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

                Console.WriteLine("MESSAGE ARRIVED");
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                Console.WriteLine("MESSAGE BODY:");
                Console.WriteLine(json);

                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<AccountDbContext>();

                if (string.IsNullOrEmpty(ea.BasicProperties.MessageId))
                {
                    _logger.LogWarning("Message without MessageId");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var messageId = Guid.Parse(ea.BasicProperties.MessageId);

                var typeName = ea.RoutingKey switch
                {
                    "user.created" => "UserCreatedIntegrationEvent",
                    "user.deleted" => "UserDeletedIntegrationEvent",
                    "user.activated" => "UserActivatedIntegrationEvent",
                    "user.deactivated" => "UserDeactivatedIntegrationEvent",
                    _ => null
                };

                if (typeName == null)
                {
                    _logger.LogWarning("Invalid type {Type}", typeName);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (!EventTypes.TryGetValue(typeName, out var type))
                {
                    _logger.LogWarning("Unknown event type {Type}", typeName);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                if (await db.InboxMessages.AnyAsync(x => x.Id == messageId))
                {
                    _logger.LogInformation("Duplicate message {MessageId}", messageId);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var inbox = new InboxMessage
                {
                    Id = messageId,
                    Type = typeName,
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
                _logger.LogError(ex, "Error processing message");

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

        _logger.LogInformation("AccountEventsConsumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}