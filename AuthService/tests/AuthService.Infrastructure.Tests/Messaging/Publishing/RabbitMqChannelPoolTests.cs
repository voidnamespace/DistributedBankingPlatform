using AuthService.Infrastructure.Messaging.Publishing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Xunit;

namespace AuthService.Infrastructure.Tests.Messaging.Publishing;

public class RabbitMqChannelPoolTests
{
    [Fact]
    public async Task RentAsync_WhenPoolIsEmpty_ShouldCreateChannelThroughFactory()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = CreateOpenChannelMock();

        connectionMock
            .Setup(connection => connection.CreateModel())
            .Returns(channelMock.Object);

        var pool = CreatePool(() => connectionMock.Object);

        // Act
        using var lease = await pool.RentAsync(CancellationToken.None);

        // Assert
        lease.Channel.Should().BeSameAs(channelMock.Object);
        connectionMock.Verify(connection => connection.CreateModel(), Times.Once);
        channelMock.Verify(channel => channel.ConfirmSelect(), Times.Once);
        channelMock.Verify(
            channel => channel.ExchangeDeclare(
                "auth.exchange",
                ExchangeType.Topic,
                true,
                false,
                It.IsAny<IDictionary<string, object>>()),
            Times.Once);
    }

    [Fact]
    public async Task DisposeLease_ShouldReturnChannelBackToPool()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = CreateOpenChannelMock();

        connectionMock
            .Setup(connection => connection.CreateModel())
            .Returns(channelMock.Object);

        var pool = CreatePool(() => connectionMock.Object);
        var lease = await pool.RentAsync(CancellationToken.None);

        // Act
        lease.Dispose();
        pool.Dispose();

        // Assert
        channelMock.Verify(channel => channel.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RentAsync_AfterChannelIsReturned_ShouldReuseSameChannel()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var channelMock = CreateOpenChannelMock();

        connectionMock
            .Setup(connection => connection.CreateModel())
            .Returns(channelMock.Object);

        var pool = CreatePool(() => connectionMock.Object);

        // Act
        IModel firstChannel;
        using (var firstLease = await pool.RentAsync(CancellationToken.None))
        {
            firstChannel = firstLease.Channel;
        }

        using var secondLease = await pool.RentAsync(CancellationToken.None);

        // Assert
        secondLease.Channel.Should().BeSameAs(firstChannel);
        connectionMock.Verify(connection => connection.CreateModel(), Times.Once);
    }

    [Fact]
    public async Task Dispose_WhenPoolContainsMultipleChannels_ShouldDisposeAllChannels()
    {
        // Arrange
        var connectionMock = new Mock<IConnection>();
        var firstChannelMock = CreateOpenChannelMock();
        var secondChannelMock = CreateOpenChannelMock();
        var createdChannels = new Queue<IModel>(new[]
        {
            firstChannelMock.Object,
            secondChannelMock.Object
        });

        connectionMock
            .Setup(connection => connection.CreateModel())
            .Returns(() => createdChannels.Dequeue());

        var pool = CreatePool(() => connectionMock.Object, maxSize: 2);

        var firstLease = await pool.RentAsync(CancellationToken.None);
        var secondLease = await pool.RentAsync(CancellationToken.None);

        firstLease.Dispose();
        secondLease.Dispose();

        // Act
        pool.Dispose();

        // Assert
        firstChannelMock.Verify(channel => channel.Dispose(), Times.Once);
        secondChannelMock.Verify(channel => channel.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RentAsync_WithConcurrentAccess_ShouldNotCreateMoreChannelsThanPoolSize()
    {
        // Arrange
        const int poolSize = 2;
        const int concurrentRequests = 6;

        var connectionMock = new Mock<IConnection>();
        var createdChannelCount = 0;
        var channelMocks = new List<Mock<IModel>>();

        connectionMock
            .Setup(connection => connection.CreateModel())
            .Returns(() =>
            {
                Interlocked.Increment(ref createdChannelCount);
                var channelMock = CreateOpenChannelMock();
                lock (channelMocks)
                {
                    channelMocks.Add(channelMock);
                }

                return channelMock.Object;
            });

        var pool = CreatePool(() => connectionMock.Object, maxSize: poolSize);
        var releaseLeases = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        // Act
        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(async _ =>
            {
                using var lease = await pool.RentAsync(CancellationToken.None);
                await releaseLeases.Task;
            })
            .ToArray();

        await Task.Delay(150);
        releaseLeases.SetResult();
        await Task.WhenAll(tasks);

        // Assert
        createdChannelCount.Should().Be(poolSize);
        connectionMock.Verify(connection => connection.CreateModel(), Times.Exactly(poolSize));
    }

    private static RabbitMqChannelPool CreatePool(Func<IConnection> connectionFactory, int maxSize = 1)
    {
        return new RabbitMqChannelPool(
            connectionFactory,
            "auth.exchange",
            maxSize,
            (_, _) => { },
            Mock.Of<ILogger<RabbitMqChannelPool>>());
    }

    private static Mock<IModel> CreateOpenChannelMock()
    {
        var channelMock = new Mock<IModel>();
        channelMock.SetupGet(channel => channel.IsOpen).Returns(true);
        channelMock.Setup(channel => channel.ConfirmSelect());
        channelMock
            .Setup(channel => channel.ExchangeDeclare(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>()));

        return channelMock;
    }
}
