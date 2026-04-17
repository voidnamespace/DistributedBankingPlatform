using AuthService.Application.Interfaces.Messaging;
using AuthService.Infrastructure.Messaging.Options;
using AuthService.Infrastructure.Messaging.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private static readonly TimeSpan PublishConfirmTimeout = TimeSpan.FromSeconds(5);

    private readonly ConnectionFactory _factory;
    private readonly RabbitMqOptions _connectionOptions;
    private readonly AuthEventsPublisherOptions _publisherOptions;

    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly RabbitMqChannelPool _channelPool;
    private readonly object _connectionLock = new();
    private readonly ConcurrentDictionary<string, ReturnedMessageInfo> _returnedMessages = new();

    private IConnection? _connection;

    public RabbitMqEventPublisher(
        IOptions<RabbitMqOptions> connectionOptions,
        IOptions<AuthEventsPublisherOptions> publisherOptions,
        ILoggerFactory loggerFactory,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _connectionOptions = connectionOptions.Value;
        _publisherOptions = publisherOptions.Value;

        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = _connectionOptions.Host,
            Port = _connectionOptions.Port,
            UserName = _connectionOptions.Username,
            Password = _connectionOptions.Password,
            DispatchConsumersAsync = true
        };

        _channelPool = new RabbitMqChannelPool(
            EnsureConnected,
            _publisherOptions.Exchange,
            _connectionOptions.ChannelPoolMaxSize,
            OnMessageReturned,
            loggerFactory.CreateLogger<RabbitMqChannelPool>());

        _ = EnsureConnected();
    }

    public async Task PublishAsync<T>(
        T message,
        string? messageId = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            ArgumentNullException.ThrowIfNull(message);

            var routingKey = IntegrationEventMap.GetName(message.GetType());
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            using var lease = await _channelPool.RentAsync(ct);
            var channel = lease.Channel;

            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            props.MessageId = string.IsNullOrWhiteSpace(messageId)
                ? Guid.NewGuid().ToString("D")
                : messageId;
            props.ContentType = "application/json";

            _returnedMessages.TryRemove(props.MessageId, out _);

            channel.BasicPublish(
                exchange: _publisherOptions.Exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body);

            var confirmed = channel.WaitForConfirms(PublishConfirmTimeout);

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
        _channelPool.Dispose();
        DisposeConnection();

        _returnedMessages.Clear();

        _logger.LogInformation("RabbitMQ publisher disposed");
    }

    private IConnection EnsureConnected()
    {
        lock (_connectionLock)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            DisposeConnectionCore();

            _connection = _factory.CreateConnection();

            _logger.LogInformation(
                "RabbitMQ publisher connection initialized. Host={Host} Port={Port}",
                _connectionOptions.Host,
                _connectionOptions.Port);

            return _connection;
        }
    }

    private void DisposeConnection()
    {
        lock (_connectionLock)
        {
            DisposeConnectionCore();
        }
    }

    private void DisposeConnectionCore()
    {
        if (_connection == null)
        {
            return;
        }

        try
        {
            _connection.Dispose();
        }
        finally
        {
            _connection = null;
        }
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
