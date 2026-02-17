using BankCardService.Application.Commands.CreateBankCard;
using BankCardService.Application.Events;
using MediatR;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BankCardService.Infrastructure.Messaging;

public class UserCreatedConsumer : BackgroundService
{
    private readonly IMediator _mediator;

    public UserCreatedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: "user.events",
                type: ExchangeType.Fanout,
                durable: true
            );

            channel.QueueDeclare(
                queue: "bankcard.user.created",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            channel.QueueBind(
                queue: "bankcard.user.created",
                exchange: "user.events",
                routingKey: ""
            );

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var messageId = ea.BasicProperties.MessageId;
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var message = JsonSerializer.Deserialize<UserCreatedEvent>(json);

                    Console.WriteLine($"📩 MessageId: {messageId}");
                    Console.WriteLine($"🔥 USER CREATED RECEIVED: {message!.UserId}");

                    var command = new CreateBankCardCommand(message.UserId);
                    await _mediator.Send(command, stoppingToken);

                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    channel.BasicNack(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: true
                    );
                }
            };

            channel.BasicConsume(
                queue: "bankcard.user.created",
                autoAck: false,
                consumer: consumer
            );

            Console.WriteLine("💡 UserCreatedConsumer запущен и слушает события...");

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine("⚠️ RabbitMQ недоступен, консьюмер пропущен: " + ex.Message);
            return Task.CompletedTask;
        }
    }
}
