using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using TransactionService.Application.Interfaces.Messaging;
using TransactionService.Infrastructure.Messaging.Options;
using TransactionService.Infrastructure.Messaging.Routing;

namespace TransactionService.Infrastructure.Messaging.Consuming;

public class AccountEventsConsumer : BackgroundService
{

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<AccountEventsConsumerOptions> _consumerOptions;
    private readonly IOptions<RabbitMqOptions> _connectionOptions;
    private readonly ILogger<AccountEventsConsumer> _logger;

    private  IConnection? _connection;
    private  IModel? _channel;

    public AccountEventsConsumer(IServiceScopeFactory scopeFactory,
        IOptions<AccountEventsConsumerOptions> consumerOptions,
        IOptions<RabbitMqOptions> connectionOptions,
        ILogger<AccountEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _consumerOptions = consumerOptions;
        _connectionOptions = connectionOptions;
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
                    HostName = _connectionOptions.Value.Host,
                    UserName = _connectionOptions.Value.Username,
                    Password = _connectionOptions.Value.Password,
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
            exchange: _consumerOptions.Value.Exchange,
            type: ExchangeType.Topic,
            durable: true
            );

        _channel.QueueDeclare(
            queue: _consumerOptions.Value.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
           queue: _consumerOptions.Value.Queue,
           exchange: _consumerOptions.Value.Exchange,
           routingKey: "transfer.*");

        _channel.QueueBind(
           queue: _consumerOptions.Value.Queue,
           exchange: _consumerOptions.Value.Exchange,
           routingKey: "withdrawal.*");

        _channel.QueueBind(
           queue: _consumerOptions.Value.Queue,
           exchange: _consumerOptions.Value.Exchange,
           routingKey: "deposit.*");


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
                        .GetRequiredService<IInboxWriter>();

                if (string.IsNullOrEmpty(ea.BasicProperties.MessageId))
                {
                    _logger.LogWarning("Message without MessageId");
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var messageId = Guid.Parse(ea.BasicProperties.MessageId);

                var typeName = ea.RoutingKey;

                if (typeName == null)
                {
                    _logger.LogWarning("Invalid type {Type}", typeName);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                var type = IntegrationEventMap.GetType(ea.RoutingKey);

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
        queue: _consumerOptions.Value.Queue,
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
