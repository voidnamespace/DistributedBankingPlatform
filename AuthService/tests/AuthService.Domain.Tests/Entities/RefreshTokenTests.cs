using AuthService.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldCreateToken()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.Token.Should().Be(tokenValue);
    }

    [Fact]
    public void Constructor_ShouldCreateUserId()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.UserId.Should().Be(userId);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var before = DateTime.UtcNow;

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.CreatedAt.Should().BeOnOrAfter(before);
        token.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldSetExpiresAt()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void Constructor_ShouldSetRevokedAtToNull()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenTokenIsStillValid()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));

        // Act
        var result = token.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenTokenHasExpired()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5));

        // Act
        var result = token.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnTrue_WhenTokenIsNotRevokedAndNotExpired()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));

        // Act
        var result = token.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenTokenIsRevoked()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));
        token.Revoke();

        // Act
        var result = token.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_ShouldReturnFalse_WhenTokenHasExpired()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5));

        // Act
        var result = token.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAt()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));
        var before = DateTime.UtcNow;

        // Act
        token.Revoke();

        // Assert
        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeOnOrAfter(before);
        token.RevokedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Revoke_ShouldMakeTokenInactive()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));

        // Act
        token.Revoke();

        // Assert
        token.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenCalledTwice_ShouldNotChangeStateAgain()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));
        token.Revoke();
        var firstRevokedAt = token.RevokedAt;
        var firstIsRevoked = token.IsRevoked;

        // Act
        token.Revoke();

        // Assert
        token.IsRevoked.Should().Be(firstIsRevoked);
        token.RevokedAt.Should().Be(firstRevokedAt);
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAtCorrectly()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var before = DateTime.UtcNow;
        var expiresAt = before.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.CreatedAt.Should().BeOnOrAfter(before);
        token.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_ShouldSetExpiresAtGreaterThanCreatedAt()
    {
        // Arrange
        const string tokenValue = "refresh-token-value";
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token = new RefreshToken(tokenValue, userId, expiresAt);

        // Assert
        token.ExpiresAt.Should().BeAfter(token.CreatedAt);
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtOnlyOnce()
    {
        // Arrange
        var token = new RefreshToken("refresh-token-value", Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5));

        // Act
        token.Revoke();
        var firstRevokedAt = token.RevokedAt;
        token.Revoke();

        // Assert
        token.RevokedAt.Should().Be(firstRevokedAt);
    }
}
