using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.IntegrationEvents;
using AccountService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
namespace AccountService.Infrastructure.Messaging;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ExchangeName = "auth.events";
    private const string QueueName = "account.user.created";
    private const string RoutingKey = "user.created";

    public UserCreatedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        IMediator mediator)
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

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                var message =
                    JsonSerializer.Deserialize<UserCreatedIntegrationEvent>(json);

                if (message is null)
                    return;

                if (!Enum.TryParse<Currency>(
                        message.Currency,
                        ignoreCase: true,
                        out var currency))
                {
                    throw new Exception($"Invalid currency: {message.Currency}");
                }

                using var scope = _scopeFactory.CreateScope();

                var mediator =
                    scope.ServiceProvider.GetRequiredService<IMediator>();

                await mediator.Send(
                    new CreateAccountCommand(
                        message.UserId,
                        currency),
                    stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch
            {
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };


        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        Console.WriteLine("UserCreatedConsumer started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
