using AuthService.Application.Commands.ActivateUser;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.ActivateUser;

public class ActivateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldActivateUserAndSaveChanges()
    {
        // Arrange
        var user = CreateInactiveUser();
        var command = new ActivateUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        user.IsActive.Should().BeTrue();
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyActiveUser_ShouldKeepUserActiveAndSaveChanges()
    {
        // Arrange
        var user = CreateActiveUser();
        var command = new ActivateUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        user.IsActive.Should().BeTrue();
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ShouldThrowKeyNotFoundExceptionAndNotSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new ActivateUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found");

        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(userId, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private ActivateUserHandler CreateHandler()
    {
        return new ActivateUserHandler(
            _userRepositoryMock.Object,
            NullLogger<ActivateUserHandler>.Instance,
            _unitOfWorkMock.Object);
    }

    private static User CreateActiveUser()
    {
        return new User(
            new EmailVO("alice@example.com"),
            new PasswordVO("SecurePassword123"));
    }

    private static User CreateInactiveUser()
    {
        var user = CreateActiveUser();
        user.Deactivate();
        return user;
    }
}
