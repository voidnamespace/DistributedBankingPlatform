using AuthService.Application.Commands.RegisterUser;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AuthService.Application.Tests.Commands.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterUserCommand("alice@example.com", "SecurePassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveValidationErrorForEmail()
    {
        // Arrange
        var command = new RegisterUserCommand("invalid-email", "SecurePassword123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidPassword_ShouldHaveValidationErrorForPassword()
    {
        // Arrange
        var command = new RegisterUserCommand("alice@example.com", "123");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithEmptyRequiredFields_ShouldHaveValidationErrors()
    {
        // Arrange
        var command = new RegisterUserCommand(string.Empty, string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
