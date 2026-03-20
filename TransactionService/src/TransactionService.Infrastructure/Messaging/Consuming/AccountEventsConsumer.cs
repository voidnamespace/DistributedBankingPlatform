using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TransactionService.Application.IntegrationEvents;
using TransactionService.Infrastructure.Messaging.Options;
using TransactionService.Infrastructure.Persistence.Inbox;

namespace TransactionService.Infrastructure.Messaging.Consuming;

public class AccountEventsConsumer : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AccountEventsConsumerOptions _options;
    private readonly ILogger<AccountEventsConsumer> _logger;
   
    
    private  IConnection? _connection;
    private  IModel? _channel;

    private static readonly Dictionary<string, Type> EventTypes = new()
    {
        ["transfer.failed"] = typeof(TransferFailedIntegrationEvent),
        ["transfer.success"] = typeof(TransferSuccessIntegrationEvent)
    };
    public AccountEventsConsumer(IServiceScopeFactory scopeFactory,
        AccountEventsConsumerOptions options,
        ILogger<AccountEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
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
                    DispatchConsumersAsync = true,
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
            durable: true
            );

        _channel.QueueDeclare(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                _logger.LogInformation(
                    "RabbitMQ message received. RoutingKey: {RoutingKey}, Body: {Body}",
                    ea.RoutingKey,
                    json);

                using var scope = _scopeFactory.CreateScope();

                var inboxWriter = scope.ServiceProvider
                    .GetRequiredService<InboxWriter>();

                if (string.IsNullOrEmpty(ea.BasicProperties.MessageId))
                {
                    _logger.LogWarning("Message without MessageId");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var messageId = Guid.Parse(ea.BasicProperties.MessageId);

                var typeName = ea.RoutingKey switch
                {
                    "transfer.created" => "TransferCreatedIntegrationEvent",
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

                await inboxWriter.SaveAsync(messageId, typeName, json, stoppingToken);

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
