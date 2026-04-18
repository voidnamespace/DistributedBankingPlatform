using AuthService.Application.Commands.DeleteUser;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.DeleteUser;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithExistingUser_ShouldDeleteUserAndSaveChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeleteUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.DeleteAsync(user, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(user, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldAddUserDeletedDomainEventBeforeDeleting()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();
        var command = new DeleteUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.DeleteAsync(user, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        user.DomainEvents.Should().ContainSingle(
            domainEvent => domainEvent.GetType().Name == "UserDeletedDomainEvent");
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ShouldThrowKeyNotFoundExceptionAndNotDeleteOrSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeleteUserCommand(userId);
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
        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private DeleteUserHandler CreateHandler()
    {
        return new DeleteUserHandler(
            _userRepositoryMock.Object,
            NullLogger<DeleteUserHandler>.Instance,
            _unitOfWorkMock.Object);
    }

    private static User CreateUser()
    {
        return new User(
            new EmailVO("alice@example.com"),
            new PasswordVO("SecurePassword123"));
    }
}
