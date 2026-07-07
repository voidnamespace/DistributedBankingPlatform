using AuthService.Application.Commands.RotateRefreshToken;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.RotateRefreshToken;

public class RotateRefreshTokenHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenHasher> _refreshTokenHasherMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithActiveRefreshTokenAndActiveUser_ShouldRotateTokenAndReturnExpectedResult()
    {
        // Arrange
        var user = CreateActiveUser();
        const string oldRefreshToken = "old-refresh-token";
        const string oldRefreshTokenHash = "old-refresh-token-hash";
        const string newRefreshToken = "new-refresh-token";
        const string newRefreshTokenHash = "new-refresh-token-hash";
        var existingRefreshToken = new RefreshToken(
            oldRefreshTokenHash,
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(oldRefreshToken);
        var cancellationToken = CancellationToken.None;
        RefreshToken? createdRefreshToken = null;
        var startedAt = DateTime.UtcNow;
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(oldRefreshToken))
            .Returns(oldRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken))
            .ReturnsAsync(existingRefreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.UpdateAsync(existingRefreshToken, cancellationToken))
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns(new AccessTokenResult("new-access-token", accessTokenExpiresAt));

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(newRefreshToken))
            .Returns(newRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<RefreshToken>(), cancellationToken))
            .Callback<RefreshToken, CancellationToken>((refreshToken, _) => createdRefreshToken = refreshToken)
            .ReturnsAsync((RefreshToken refreshToken, CancellationToken _) => refreshToken);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        existingRefreshToken.IsRevoked.Should().BeTrue();
        existingRefreshToken.RevokedAt.Should().NotBeNull();

        _refreshTokenRepositoryMock.Verify(
            repository => repository.GetByTokenHashAsync(oldRefreshTokenHash, cancellationToken),
            Times.Once);
        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(user.Id, cancellationToken),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(existingRefreshToken, cancellationToken),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);

        createdRefreshToken.Should().NotBeNull();
        createdRefreshToken!.UserId.Should().Be(user.Id);
        createdRefreshToken.Token.Should().Be(newRefreshTokenHash);
        createdRefreshToken.IsRevoked.Should().BeFalse();
        createdRefreshToken.ExpiryDate.Should().BeAfter(startedAt.AddDays(6));

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be(newRefreshToken);
        result.ExpiresAt.Should().Be(accessTokenExpiresAt);
    }

    [Fact]
    public async Task Handle_WithUnknownRefreshToken_ShouldThrowUnauthorizedAccessExceptionAndNotModifyState()
    {
        // Arrange
        var command = new RotateRefreshTokenCommand("missing-token");
        var cancellationToken = CancellationToken.None;
        const string missingTokenHash = "missing-token-hash";

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(command.RefreshToken))
            .Returns(missingTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(missingTokenHash, cancellationToken))
            .ReturnsAsync((RefreshToken?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid refresh token");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveRefreshToken_ShouldThrowUnauthorizedAccessExceptionAndNotRotateToken()
    {
        // Arrange
        const string revokedToken = "revoked-token";
        const string revokedTokenHash = "revoked-token-hash";
        var refreshToken = new RefreshToken(
            revokedTokenHash,
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(5));
        refreshToken.Revoke();
        var command = new RotateRefreshTokenCommand(revokedToken);
        var cancellationToken = CancellationToken.None;

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(command.RefreshToken))
            .Returns(revokedTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(revokedTokenHash, cancellationToken))
            .ReturnsAsync(refreshToken);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is invalid or revoked");

        _userRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithMissingUser_ShouldThrowUnauthorizedAccessExceptionAndNotRotateToken()
    {
        // Arrange
        const string validRefreshToken = "valid-refresh-token";
        const string validRefreshTokenHash = "valid-refresh-token-hash";
        var refreshToken = new RefreshToken(
            validRefreshTokenHash,
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(validRefreshToken);
        var cancellationToken = CancellationToken.None;

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(command.RefreshToken))
            .Returns(validRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(validRefreshTokenHash, cancellationToken))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(refreshToken.UserId, cancellationToken))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found or inactive");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedAccessExceptionAndNotRotateToken()
    {
        // Arrange
        var user = CreateInactiveUser();
        const string validRefreshToken = "valid-refresh-token";
        const string validRefreshTokenHash = "valid-refresh-token-hash";
        var refreshToken = new RefreshToken(
            validRefreshTokenHash,
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(validRefreshToken);
        var cancellationToken = CancellationToken.None;

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(command.RefreshToken))
            .Returns(validRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(validRefreshTokenHash, cancellationToken))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found or inactive");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenConcurrentReuseIsDetected_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var user = CreateActiveUser();
        const string validRefreshToken = "valid-refresh-token";
        const string validRefreshTokenHash = "valid-refresh-token-hash";
        const string newRefreshToken = "new-refresh-token";
        const string newRefreshTokenHash = "new-refresh-token-hash";
        var refreshToken = new RefreshToken(
            validRefreshTokenHash,
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(validRefreshToken);
        var cancellationToken = CancellationToken.None;

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(command.RefreshToken))
            .Returns(validRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenHashAsync(validRefreshTokenHash, cancellationToken))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.UpdateAsync(refreshToken, cancellationToken))
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns(new AccessTokenResult("new-access-token", DateTime.UtcNow.AddMinutes(15)));

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _refreshTokenHasherMock
            .Setup(hasher => hasher.Hash(newRefreshToken))
            .Returns(newRefreshTokenHash);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<RefreshToken>(), cancellationToken))
            .ReturnsAsync((RefreshToken createdToken, CancellationToken _) => createdToken);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .ThrowsAsync(new Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException());

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token is invalid or revoked");
    }

    private RotateRefreshTokenHandler CreateHandler()
    {
        return new RotateRefreshTokenHandler(
            _refreshTokenRepositoryMock.Object,
            NullLogger<RotateRefreshTokenHandler>.Instance,
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenHasherMock.Object,
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
