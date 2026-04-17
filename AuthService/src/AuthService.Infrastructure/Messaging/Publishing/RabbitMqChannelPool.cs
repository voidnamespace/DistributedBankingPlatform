using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace AuthService.Infrastructure.Messaging.Publishing;

public sealed class RabbitMqChannelPool : IDisposable
{
    private readonly Func<IConnection> _connectionFactory;
    private readonly string _exchange;
    private readonly ILogger<RabbitMqChannelPool> _logger;
    private readonly EventHandler<BasicReturnEventArgs> _basicReturnHandler;
    private readonly ConcurrentBag<IModel> _channels = new();
    private readonly SemaphoreSlim _slots;

    private int _disposed;

    public RabbitMqChannelPool(
        Func<IConnection> connectionFactory,
        string exchange,
        int maxSize,
        EventHandler<BasicReturnEventArgs> basicReturnHandler,
        ILogger<RabbitMqChannelPool> logger)
    {
        if (maxSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxSize),
                maxSize,
                "RabbitMQ channel pool size must be greater than zero.");
        }

        _connectionFactory = connectionFactory;
        _exchange = exchange;
        _basicReturnHandler = basicReturnHandler;
        _logger = logger;
        _slots = new SemaphoreSlim(maxSize, maxSize);
    }

    public async Task<ChannelLease> RentAsync(CancellationToken ct)
    {
        ThrowIfDisposed();

        await _slots.WaitAsync(ct);

        try
        {
            var channel = GetOrCreateChannel();
            return new ChannelLease(this, channel);
        }
        catch
        {
            _slots.Release();
            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        while (_channels.TryTake(out var channel))
        {
            DisposeChannel(channel, "pool disposal");
        }

        _logger.LogInformation("RabbitMQ channel pool disposed");
    }

    private IModel GetOrCreateChannel()
    {
        while (_channels.TryTake(out var channel))
        {
            if (IsOpen(channel))
            {
                return channel;
            }

            DisposeChannel(channel, "closed pooled channel");

            _logger.LogWarning(
                "RabbitMQ pooled channel was closed and will be recreated");
        }

        return CreateChannel();
    }

    private IModel CreateChannel()
    {
        var connection = _connectionFactory();
        var channel = connection.CreateModel();

        channel.ConfirmSelect();
        channel.ExchangeDeclare(
            exchange: _exchange,
            type: ExchangeType.Topic,
            durable: true);
        channel.BasicReturn += _basicReturnHandler;

        _logger.LogDebug(
            "RabbitMQ channel created and configured for exchange {Exchange}",
            _exchange);

        return channel;
    }

    private void Return(IModel channel)
    {
        if (Volatile.Read(ref _disposed) == 1)
        {
            DisposeChannel(channel, "pool already disposed");
            _slots.Release();
            return;
        }

        if (IsOpen(channel))
        {
            _channels.Add(channel);
        }
        else
        {
            DisposeChannel(channel, "closed channel on return");

            _logger.LogWarning(
                "RabbitMQ channel returned to pool in closed state and was disposed");
        }

        _slots.Release();
    }

    private void DisposeChannel(IModel? channel, string reason)
    {
        if (channel == null)
        {
            return;
        }

        try
        {
            channel.BasicReturn -= _basicReturnHandler;
            channel.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "RabbitMQ channel dispose failed. Reason={Reason}",
                reason);
        }
    }

    private static bool IsOpen(IModel? channel)
        => channel is { IsOpen: true };

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) == 1,
            this);
    }

    public sealed class ChannelLease : IDisposable
    {
        private readonly RabbitMqChannelPool _owner;
        private int _disposed;

        internal ChannelLease(RabbitMqChannelPool owner, IModel channel)
        {
            _owner = owner;
            Channel = channel;
        }

        public IModel Channel { get; }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            _owner.Return(Channel);
        }
    }
}
