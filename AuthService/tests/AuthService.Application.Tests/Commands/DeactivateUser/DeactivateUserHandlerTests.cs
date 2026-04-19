using AuthService.Application.Commands.DeactivateUser;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.DeactivateUser;

public class DeactivateUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();

    [Fact]
    public async Task Handle_WithActiveUser_ShouldDeactivateUserRevokeTokensAndSaveChanges()
    {
        // Arrange
        var user = CreateActiveUser();
        var command = new DeactivateUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.RevokeAllUserTokensAsync(user.Id, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        user.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllUserTokensAsync(user.Id, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveUser_ShouldKeepUserInactiveRevokeTokensAndSaveChanges()
    {
        // Arrange
        var user = CreateInactiveUser();
        var command = new DeactivateUserCommand(user.Id);
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.RevokeAllUserTokensAsync(user.Id, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        user.IsActive.Should().BeFalse();
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllUserTokensAsync(user.Id, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ShouldThrowKeyNotFoundExceptionAndNotRevokeTokensOrSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new DeactivateUserCommand(userId);
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
        _refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private DeactivateUserHandler CreateHandler()
    {
        return new DeactivateUserHandler(
            _userRepositoryMock.Object,
            NullLogger<DeactivateUserHandler>.Instance,
            _unitOfWorkMock.Object,
            _refreshTokenRepositoryMock.Object);
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
