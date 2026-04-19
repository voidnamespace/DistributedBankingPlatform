using AuthService.Application.Commands.RegisterUser;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.RegisterUser;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserSaveChangesAndReturnExpectedResult()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "alice@example.com",
            "SecurePassword123");
        var cancellationToken = CancellationToken.None;
        User? createdUser = null;

        _userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<User>(), cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);

        createdUser.Should().NotBeNull();
        result.UserId.Should().Be(createdUser!.Id);
        result.Email.Should().Be(command.Email);
        result.Message.Should().Be("Registration was successful");
    }

    [Fact]
    public async Task Handle_WithEmailContainingOuterSpaces_ShouldTrimEmailBeforeCreatingUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "  alice@example.com  ",
            "SecurePassword123");
        var cancellationToken = CancellationToken.None;
        User? createdUser = null;

        _userRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<User>(), cancellationToken))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        createdUser.Should().NotBeNull();
        createdUser!.Email.Value.Should().Be("alice@example.com");
        result.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrowArgumentExceptionFromEmailVO()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "invalid-email",
            "SecurePassword123");
        var cancellationToken = CancellationToken.None;
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _userRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowArgumentExceptionFromPasswordVO()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "alice@example.com",
            "123");
        var cancellationToken = CancellationToken.None;
        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
        _userRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private RegisterUserHandler CreateHandler()
    {
        return new RegisterUserHandler(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            NullLogger<RegisterUserHandler>.Instance);
    }
}
