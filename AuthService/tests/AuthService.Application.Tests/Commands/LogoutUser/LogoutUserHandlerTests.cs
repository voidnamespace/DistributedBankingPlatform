using AuthService.Application.Commands.LogoutUser;
using AuthService.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.LogoutUser;

public class LogoutUserHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithValidUserId_ShouldRevokeAllTokensAndSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.RevokeAllUserTokensAsync(userId, cancellationToken))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        _refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllUserTokensAsync(userId, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRevokingTokensFails_ShouldPropagateExceptionAndNotSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new LogoutUserCommand(userId);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.RevokeAllUserTokensAsync(userId, cancellationToken))
            .ThrowsAsync(new InvalidOperationException("Revoke failed"));

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Revoke failed");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.RevokeAllUserTokensAsync(userId, cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private LogoutUserHandler CreateHandler()
    {
        return new LogoutUserHandler(
            _refreshTokenRepositoryMock.Object,
            NullLogger<LogoutUserHandler>.Instance,
            _unitOfWorkMock.Object);
    }
}
