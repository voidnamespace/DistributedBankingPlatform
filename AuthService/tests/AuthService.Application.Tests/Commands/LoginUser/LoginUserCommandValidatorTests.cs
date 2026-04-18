using AuthService.Application.Commands.LoginUser;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AuthService.Application.Tests.Commands.LoginUser;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new LoginUserCommand("alice@example.com", "SecurePassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveValidationErrorForEmail()
    {
        // Arrange
        var command = new LoginUserCommand("invalid-email", "SecurePassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidPassword_ShouldHaveValidationErrorForPassword()
    {
        // Arrange
        var command = new LoginUserCommand("alice@example.com", "123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new LoginUserCommand(string.Empty, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
