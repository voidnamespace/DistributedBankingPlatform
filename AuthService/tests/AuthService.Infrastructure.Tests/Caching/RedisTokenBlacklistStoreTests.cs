using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Caching;
using FluentAssertions;
using Moq;
using Xunit;

namespace AuthService.Infrastructure.Tests.Caching;

public class RedisTokenBlacklistStoreTests
{
    private readonly Mock<IRedisService> _redisServiceMock = new();

    [Fact]
    public async Task IsBlacklistedAsync_WhenRedisReadFails_ShouldPropagateException()
    {
        // Arrange
        var store = new RedisTokenBlacklistStore(_redisServiceMock.Object);
        var exception = new InvalidOperationException("Redis unavailable");

        _redisServiceMock
            .Setup(redis => redis.GetAsync<bool>("blacklist:access-token:token-id"))
            .ThrowsAsync(exception);

        // Act
        var act = async () => await store.IsBlacklistedAsync(
            "token-id",
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Redis unavailable");
    }
}
