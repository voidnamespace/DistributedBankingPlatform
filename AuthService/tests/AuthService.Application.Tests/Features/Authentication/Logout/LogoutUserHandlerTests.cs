using AuthService.Application.Abstractions.Authentication;
using AuthService.Application.Abstractions.Persistence;
using AuthService.Application.Features.Authentication.Logout;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Features.Authentication.Logout;

public class LogoutUserHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITokenBlacklistStore> _tokenBlacklistStoreMock = new();

    [Fact]
    public async Task Handle_WithValidUserId_ShouldRevokeAllTokensAndSaveChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string accessTokenId = "access-token-id";
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        var command = new LogoutUserCommand(
            userId,
            accessTokenId,
            accessTokenExpiresAt);
        var cancellationToken = CancellationToken.None;

        _tokenBlacklistStoreMock
            .Setup(store => store.BlacklistAsync(
                accessTokenId,
                It.Is<TimeSpan>(expiresIn => expiresIn > TimeSpan.Zero),
                cancellationToken))
            .Returns(Task.CompletedTask);

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
        _tokenBlacklistStoreMock.Verify(
            store => store.BlacklistAsync(
                accessTokenId,
                It.Is<TimeSpan>(expiresIn => expiresIn > TimeSpan.Zero),
                cancellationToken),
            Times.Once);
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
        const string accessTokenId = "access-token-id";
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        var command = new LogoutUserCommand(
            userId,
            accessTokenId,
            accessTokenExpiresAt);
        var cancellationToken = CancellationToken.None;

        _tokenBlacklistStoreMock
            .Setup(store => store.BlacklistAsync(
                accessTokenId,
                It.Is<TimeSpan>(expiresIn => expiresIn > TimeSpan.Zero),
                cancellationToken))
            .Returns(Task.CompletedTask);

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

        _tokenBlacklistStoreMock.Verify(
            store => store.BlacklistAsync(
                accessTokenId,
                It.Is<TimeSpan>(expiresIn => expiresIn > TimeSpan.Zero),
                cancellationToken),
            Times.Once);
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
            _unitOfWorkMock.Object,
            _tokenBlacklistStoreMock.Object);
    }
}
