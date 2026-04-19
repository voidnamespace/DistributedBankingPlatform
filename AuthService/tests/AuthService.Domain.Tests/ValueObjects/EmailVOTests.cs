using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests.ValueObjects;

public class EmailVOTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateSuccessfully()
    {
        // Arrange
        const string email = "alice@example.com";

        // Act
        var result = new EmailVO(email);

        // Assert
        result.Value.Should().Be(email);
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowArgumentException()
    {
        // Arrange
        string? email = null;

        // Act
        Action act = () => _ = new EmailVO(email!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyString_ShouldThrowArgumentException()
    {
        // Arrange
        const string email = "";

        // Act
        Action act = () => _ = new EmailVO(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Constructor_WithWhitespace_ShouldThrowArgumentException(string email)
    {
        // Arrange

        // Act
        Action act = () => _ = new EmailVO(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("alice@")]
    [InlineData("@example.com")]
    [InlineData("alice.example.com")]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowArgumentException(string email)
    {
        // Arrange

        // Act
        Action act = () => _ = new EmailVO(email);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var second = new EmailVO("alice@example.com");

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeTrue();
        first.Should().Be(second);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var second = new EmailVO("bob@example.com");

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeFalse();
        first.Should().NotBe(second);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var second = new EmailVO("alice@example.com");

        // Act
        var typedEquals = first.Equals(second);
        var objectEquals = first.Equals((object)second);

        // Assert
        typedEquals.Should().BeTrue();
        objectEquals.Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValueAndNull_ShouldReturnFalse()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var second = new EmailVO("bob@example.com");

        // Act
        var equalsDifferent = first.Equals(second);
        var equalsNull = first.Equals(null);

        // Assert
        equalsDifferent.Should().BeFalse();
        equalsNull.Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_WithSameAndDifferentValues_ShouldWorkCorrectly()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var same = new EmailVO("alice@example.com");
        var different = new EmailVO("bob@example.com");

        // Act
        var sameResult = first == same;
        var differentResult = first == different;

        // Assert
        sameResult.Should().BeTrue();
        differentResult.Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithSameAndDifferentValues_ShouldWorkCorrectly()
    {
        // Arrange
        var first = new EmailVO("alice@example.com");
        var same = new EmailVO("alice@example.com");
        var different = new EmailVO("bob@example.com");

        // Act
        var sameResult = first != same;
        var differentResult = first != different;

        // Assert
        sameResult.Should().BeFalse();
        differentResult.Should().BeTrue();
    }

    [Fact]
    public void ValueProperty_ShouldReturnStoredEmail()
    {
        // Arrange
        const string email = "alice@example.com";
        var result = new EmailVO(email);

        // Act
        var value = result.Value;

        // Assert
        value.Should().Be(email);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnEmailValue()
    {
        // Arrange
        const string email = "alice@example.com";
        var result = new EmailVO(email);

        // Act
        var stringValue = result.ToString();

        // Assert
        stringValue.Should().Be(email);
    }
}
