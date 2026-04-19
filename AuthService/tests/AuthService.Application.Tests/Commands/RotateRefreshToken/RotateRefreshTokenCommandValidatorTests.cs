using AuthService.Application.Commands.RotateRefreshToken;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AuthService.Application.Tests.Commands.RotateRefreshToken;

public class RotateRefreshTokenCommandValidatorTests
{
    private readonly RotateRefreshTokenCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RotateRefreshTokenCommand("valid-refresh-token");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidRefreshToken_ShouldHaveValidationErrorForRefreshToken()
    {
        // Arrange
        var command = new RotateRefreshTokenCommand(" ");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void Validate_WithEmptyRequiredFields_ShouldHaveValidationErrorForRefreshToken()
    {
        // Arrange
        var command = new RotateRefreshTokenCommand(string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}
