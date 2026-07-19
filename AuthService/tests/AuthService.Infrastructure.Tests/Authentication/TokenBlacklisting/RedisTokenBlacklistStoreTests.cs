using AuthService.Infrastructure.Authentication.TokenBlacklisting;
using FluentAssertions;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace AuthService.Infrastructure.Tests.Authentication.TokenBlacklisting;

public class RedisTokenBlacklistStoreTests
{
    private const string ExpectedKey =
        "authservice:blacklist:access-token:token-id";

    private readonly Mock<IDatabase> _databaseMock = new();
    private readonly RedisTokenBlacklistStore _store;

    public RedisTokenBlacklistStoreTests()
    {
        var redisMock = new Mock<IConnectionMultiplexer>();
        redisMock
            .Setup(redis => redis.GetDatabase(-1, null))
            .Returns(_databaseMock.Object);

        _store = new RedisTokenBlacklistStore(redisMock.Object);
    }

    [Fact]
    public async Task BlacklistAsync_ShouldStoreMarkerWithTokenLifetime()
    {
        // Arrange
        var expiresIn = TimeSpan.FromMinutes(15);

        _databaseMock
            .Setup(database => database.StringSetAsync(
                It.Is<RedisKey>(key => key.ToString() == ExpectedKey),
                It.Is<RedisValue>(value => value.ToString() == "true"),
                expiresIn,
                When.Always,
                CommandFlags.None))
            .ReturnsAsync(true);

        // Act
        await _store.BlacklistAsync(
            "token-id",
            expiresIn,
            CancellationToken.None);

        // Assert
        _databaseMock.Verify(database => database.StringSetAsync(
                It.Is<RedisKey>(key => key.ToString() == ExpectedKey),
                It.Is<RedisValue>(value => value.ToString() == "true"),
                expiresIn,
                When.Always,
                CommandFlags.None),
            Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsBlacklistedAsync_ShouldReturnWhetherTokenKeyExists(
        bool keyExists)
    {
        // Arrange
        _databaseMock
            .Setup(database => database.KeyExistsAsync(
                It.Is<RedisKey>(key => key.ToString() == ExpectedKey),
                CommandFlags.None))
            .ReturnsAsync(keyExists);

        // Act
        var result = await _store.IsBlacklistedAsync(
            "token-id",
            CancellationToken.None);

        // Assert
        result.Should().Be(keyExists);
    }

    [Fact]
    public async Task IsBlacklistedAsync_WhenRedisReadFails_ShouldPropagateException()
    {
        // Arrange
        var exception = new InvalidOperationException("Redis unavailable");

        _databaseMock
            .Setup(database => database.KeyExistsAsync(
                It.Is<RedisKey>(key => key.ToString() == ExpectedKey),
                CommandFlags.None))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await _store.IsBlacklistedAsync(
            "token-id",
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Redis unavailable");
    }
}
