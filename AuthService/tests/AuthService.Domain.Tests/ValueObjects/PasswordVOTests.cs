using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests.ValueObjects;

public class PasswordVOTests
{
    [Fact]
    public void FromHash_WithValidHash_ShouldCreateSuccessfully()
    {
        // Arrange
        const string hash = "$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop";

        // Act
        var result = PasswordVO.FromHash(hash);

        // Assert
        result.Hash.Should().Be(hash);
    }

    [Fact]
    public void FromHash_WithNullHash_ShouldThrowArgumentException()
    {
        // Arrange
        string? hash = null;

        // Act
        Action act = () => _ = PasswordVO.FromHash(hash!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromHash_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        const string hash = "";

        // Act
        Action act = () => _ = PasswordVO.FromHash(hash);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void FromHash_WithWhitespace_ShouldThrowArgumentException(string hash)
    {
        // Arrange

        // Act
        Action act = () => _ = PasswordVO.FromHash(hash);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HashProperty_ShouldReturnStoredHash()
    {
        // Arrange
        const string hash = "$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop";
        var password = PasswordVO.FromHash(hash);

        // Act
        var result = password.Hash;

        // Assert
        result.Should().Be(hash);
    }

    [Fact]
    public void Equality_WithSameHashValues_ShouldBeEqual()
    {
        // Arrange
        const string hash = "$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop";
        var first = PasswordVO.FromHash(hash);
        var second = PasswordVO.FromHash(hash);

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeTrue();
        first.Should().Be(second);
    }

    [Fact]
    public void Equality_WithDifferentHashValues_ShouldNotBeEqual()
    {
        // Arrange
        var first = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var second = PasswordVO.FromHash("$2a$11$hash22222222222222222222222222222222222222222222");

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeFalse();
        first.Should().NotBe(second);
    }

    [Fact]
    public void Equals_WithSameHashValue_ShouldReturnTrue()
    {
        // Arrange
        const string hash = "$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop";
        var first = PasswordVO.FromHash(hash);
        var second = PasswordVO.FromHash(hash);

        // Act
        var typedEquals = first.Equals(second);
        var objectEquals = first.Equals((object)second);

        // Assert
        typedEquals.Should().BeTrue();
        objectEquals.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentHashValueAndNull_ShouldReturnFalse()
    {
        // Arrange
        var first = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var second = PasswordVO.FromHash("$2a$11$hash22222222222222222222222222222222222222222222");

        // Act
        var equalsDifferent = first.Equals(second);
        var equalsNull = first.Equals(null);

        // Assert
        equalsDifferent.Should().BeFalse();
        equalsNull.Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameHashValues_ShouldMatch()
    {
        // Arrange
        const string hash = "$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop";
        var first = PasswordVO.FromHash(hash);
        var second = PasswordVO.FromHash(hash);

        // Act
        var firstHashCode = first.GetHashCode();
        var secondHashCode = second.GetHashCode();

        // Assert
        firstHashCode.Should().Be(secondHashCode);
    }

    [Fact]
    public void EqualityOperator_WithSameAndDifferentHashValues_ShouldWorkCorrectly()
    {
        // Arrange
        var first = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var same = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var different = PasswordVO.FromHash("$2a$11$hash22222222222222222222222222222222222222222222");

        // Act
        var sameResult = first == same;
        var differentResult = first == different;

        // Assert
        sameResult.Should().BeTrue();
        differentResult.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameAndDifferentHashValues_ShouldWorkCorrectly()
    {
        // Arrange
        var first = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var same = PasswordVO.FromHash("$2a$11$hash11111111111111111111111111111111111111111111");
        var different = PasswordVO.FromHash("$2a$11$hash22222222222222222222222222222222222222222222");

        // Act
        var sameResult = first != same;
        var differentResult = first != different;

        // Assert
        sameResult.Should().BeFalse();
        differentResult.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldNotReturnRawPassword()
    {
        // Arrange
        const string rawPassword = "SuperSecret123";
        var password = PasswordVO.Create(rawPassword);

        // Act
        var stringValue = password.ToString();

        // Assert
        stringValue.Should().NotBe(rawPassword);
    }

    [Fact]
    public void ToString_ShouldReturnSafeRepresentation()
    {
        // Arrange
        var password = PasswordVO.FromHash("$2a$11$abcdefghijklmnopqrstuv1234567890ABCDEFGHijklmnop");

        // Act
        var stringValue = password.ToString();

        // Assert
        stringValue.Should().Be("PasswordVO(****)");
    }

    [Fact]
    public void FromHash_WithOriginalHash_ShouldCreateEqualPasswordObject()
    {
        // Arrange
        var original = PasswordVO.Create("SuperSecret123");

        // Act
        var recreated = PasswordVO.FromHash(original.Hash);

        // Assert
        recreated.Should().Be(original);
        recreated.Hash.Should().Be(original.Hash);
    }
}
