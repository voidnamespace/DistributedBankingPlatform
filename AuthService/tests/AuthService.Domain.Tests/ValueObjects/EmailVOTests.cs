using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace AuthService.Domain.Tests.ValueObjects;

public class EmailVOTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldStoreValue()
    {
        // Arrange
        const string email = "alice@example.com";

        // Act
        var result = new EmailVO(email);

        // Assert
        result.Value.Should().Be(email);
    }
}
