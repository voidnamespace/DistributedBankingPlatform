using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Messaging.Options;
using AuthService.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace AuthService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private static readonly TimeSpan PublishConfirmTimeout = TimeSpan.FromSeconds(5);

    private readonly RabbitMqOptions _connectionOptions;
    private readonly AuthEventsPublisherOptions _publisherOptions;

    private readonly ILogger<RabbitMqEventPublisher> _logger;

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly object _publishLock = new();
    private readonly ConcurrentDictionary<string, ReturnedMessageInfo> _returnedMessages = new();

    public RabbitMqEventPublisher(
        IOptions<RabbitMqOptions> connectionOptions,
        IOptions<AuthEventsPublisherOptions> publisherOptions,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connectionOptions = connectionOptions.Value;
        _publisherOptions = publisherOptions.Value;

        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = _connectionOptions.Host,
            Port = _connectionOptions.Port,
            UserName = _connectionOptions.Username,
            Password = _connectionOptions.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ConfirmSelect();

        _channel.ExchangeDeclare(
            exchange: _publisherOptions.Exchange,
            type: ExchangeType.Topic,
            durable: true);

        _channel.BasicReturn += OnMessageReturned;

        _logger.LogInformation(
            "RabbitMQ publisher initialized. Exchange={Exchange}",
            _publisherOptions.Exchange);
    }

    public Task PublishAsync<T>(
    T message,
    string? messageId = null,
    CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var routingKey = IntegrationEventMap.GetName(message!.GetType());

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message));

            lock (_publishLock)
            {
                var props = _channel.CreateBasicProperties();

                props.Persistent = true;
                props.MessageId = string.IsNullOrWhiteSpace(messageId)
                    ? Guid.NewGuid().ToString("D")
                    : messageId;
                props.ContentType = "application/json";

                _returnedMessages.TryRemove(props.MessageId, out _);

                _channel.BasicPublish(
                    exchange: _publisherOptions.Exchange,
                    routingKey: routingKey,
                    mandatory: true,
                    basicProperties: props,
                    body: body);

                var confirmed = _channel.WaitForConfirms(PublishConfirmTimeout);

                if (!confirmed)
                {
                    throw new InvalidOperationException(
                        $"RabbitMQ publish was not confirmed for message {props.MessageId}");
                }

                if (_returnedMessages.TryRemove(props.MessageId, out var returnedMessage))
                {
                    throw new InvalidOperationException(
                        $"RabbitMQ returned message {props.MessageId}. " +
                        $"ReplyCode={returnedMessage.ReplyCode} " +
                        $"ReplyText={returnedMessage.ReplyText} " +
                        $"RoutingKey={returnedMessage.RoutingKey}");
                }

                _logger.LogInformation(
                    "RabbitMQ event published. Type={EventType} RoutingKey={RoutingKey} MessageId={MessageId}",
                    message.GetType().Name,
                    routingKey,
                    props.MessageId);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "RabbitMQ publish failed. Type={EventType}",
                typeof(T).Name);

            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();

        _logger.LogInformation("RabbitMQ publisher disposed");
    }

    private void OnMessageReturned(object? sender, BasicReturnEventArgs args)
    {
        var messageId = args.BasicProperties?.MessageId;

        if (string.IsNullOrWhiteSpace(messageId))
        {
            _logger.LogWarning(
                "RabbitMQ message returned without MessageId. RoutingKey={RoutingKey}",
                args.RoutingKey);

            return;
        }

        _returnedMessages[messageId] = new ReturnedMessageInfo(
            args.ReplyCode,
            args.ReplyText,
            args.RoutingKey);

        _logger.LogWarning(
            "RabbitMQ message returned. MessageId={MessageId} RoutingKey={RoutingKey} ReplyCode={ReplyCode} ReplyText={ReplyText}",
            messageId,
            args.RoutingKey,
            args.ReplyCode,
            args.ReplyText);
    }

    private sealed record ReturnedMessageInfo(
        ushort ReplyCode,
        string ReplyText,
        string RoutingKey);
}
