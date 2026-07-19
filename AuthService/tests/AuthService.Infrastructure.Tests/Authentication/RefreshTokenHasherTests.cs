using AuthService.Infrastructure.Authentication;
using FluentAssertions;
using Xunit;

namespace AuthService.Infrastructure.Tests.Authentication;

public class RefreshTokenHasherTests
{
    private readonly RefreshTokenHasher _hasher = new();

    [Fact]
    public void Hash_WithSameRefreshToken_ShouldReturnSameHash()
    {
        // Arrange
        const string refreshToken = "refresh-token-value";

        // Act
        var firstHash = _hasher.Hash(refreshToken);
        var secondHash = _hasher.Hash(refreshToken);

        // Assert
        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public void Hash_WithRefreshToken_ShouldNotReturnRawRefreshToken()
    {
        // Arrange
        const string refreshToken = "refresh-token-value";

        // Act
        var hash = _hasher.Hash(refreshToken);

        // Assert
        hash.Should().NotBe(refreshToken);
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Hash_WithInvalidRefreshToken_ShouldThrowArgumentException(string refreshToken)
    {
        // Act
        var act = () => _hasher.Hash(refreshToken);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
