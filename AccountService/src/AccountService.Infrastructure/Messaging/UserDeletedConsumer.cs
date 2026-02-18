using System.Text;
using System.Text.Json;
using AccountService.Application.IntegrationEvents;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace AccountService.Infrastructure.Messaging;

public class UserDeletedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "auth.events";
    private const string QueueName = "account.user.deleted";
    private const string RoutingKey = "user.deleted";

    public UserDeletedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _config["RabbitMQ:Host"] ?? "rabbitmq",
                    UserName = _config["RabbitMQ:Username"] ?? "guest",
                    Password = _config["RabbitMQ:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                Console.WriteLine("RabbitMQ connected");
                break;
            }
            catch
            {
                Console.WriteLine("Waiting RabbitMQ...");
                await Task.Delay(3000, stoppingToken);
            }
        }

        if (_channel is null)
            return;

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<UserDeletedIntegrationEvent>(json);

                if (message is null)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();

                var repo = scope.ServiceProvider
                    .GetRequiredService<IAccountRepository>();

                var unitOfWork = scope.ServiceProvider
                    .GetRequiredService<IUnitOfWork>();

                await repo.DeleteAsync(message.UserId, stoppingToken);
                await unitOfWork.SaveChangesAsync(stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                _channel.BasicNack(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("UserDeletedConsumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
