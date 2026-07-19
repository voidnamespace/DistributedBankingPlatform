using AuthService.Application.Abstractions.ExternalServices.Accounts;
using AuthService.Application.Abstractions.ExternalServices.Accounts.Contracts;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Common.Exceptions;
using AuthService.Application.Features.Users.DeleteUser;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Features.Users.DeleteUser;

public class DeleteUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserDeletionValidator> _userDeletionValidatorMock = new();

    [Fact]
    public async Task Handle_WhenDeletionValidationSucceeds_ShouldDeleteUserAndSaveChanges()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeleteUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _userDeletionValidatorMock
            .Setup(validator => validator.ValidateUserDeletion(
                It.Is<UserDeletionValidationRequest>(request => request.UserId == user.Id)))
            .ReturnsAsync(new UserDeletionValidationResponse(true));

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

        _userDeletionValidatorMock.Verify(
            validator => validator.ValidateUserDeletion(
                It.Is<UserDeletionValidationRequest>(request => request.UserId == user.Id)),
            Times.Once);

        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(user, cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDeletionValidationSucceeds_ShouldAddUserDeletedDomainEvent()
    {
        // Arrange
        var user = CreateUser();
        user.ClearDomainEvents();
        var command = new DeleteUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _userDeletionValidatorMock
            .Setup(validator => validator.ValidateUserDeletion(
                It.Is<UserDeletionValidationRequest>(request => request.UserId == user.Id)))
            .ReturnsAsync(new UserDeletionValidationResponse(true));

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
    public async Task Handle_WhenUserDoesNotExist_ShouldThrowAndNotDeleteOrSave()
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
        _userDeletionValidatorMock.Verify(
            validator => validator.ValidateUserDeletion(
                It.IsAny<UserDeletionValidationRequest>()),
            Times.Never);
        _userRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDeletionValidationFails_ShouldThrowAndNotDeleteOrSave()
    {
        // Arrange
        var user = CreateUser();
        var command = new DeleteUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _userDeletionValidatorMock
            .Setup(validator => validator.ValidateUserDeletion(
                It.Is<UserDeletionValidationRequest>(request => request.UserId == user.Id)))
            .ReturnsAsync(new UserDeletionValidationResponse(false));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UserDeletionRejectedException>()
            .WithMessage("User cannot be deleted while account deletion validation failed.");

        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);

        _userDeletionValidatorMock.Verify(
            validator => validator.ValidateUserDeletion(
                It.Is<UserDeletionValidationRequest>(request => request.UserId == user.Id)),
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
            _unitOfWorkMock.Object,
            _userDeletionValidatorMock.Object);
    }

    private static User CreateUser()
    {
        return new User(
            new EmailVO("alice@example.com"),
            new PasswordVO("SecurePassword123"));
    }
}
