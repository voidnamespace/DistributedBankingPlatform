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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithActiveRefreshTokenAndActiveUser_ShouldRotateTokenAndReturnExpectedResult()
    {
        // Arrange
        var user = CreateActiveUser();
        var existingRefreshToken = new RefreshToken(
            "old-refresh-token",
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(existingRefreshToken.Token);
        var cancellationToken = CancellationToken.None;
        RefreshToken? createdRefreshToken = null;
        var startedAt = DateTime.UtcNow;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
            .ReturnsAsync(existingRefreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.UpdateAsync(existingRefreshToken, cancellationToken))
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns("new-access-token");

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns("new-refresh-token");

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
            repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken),
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
        createdRefreshToken.Token.Should().Be("new-refresh-token");
        createdRefreshToken.IsRevoked.Should().BeFalse();
        createdRefreshToken.ExpiryDate.Should().BeAfter(startedAt.AddDays(6));

        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.ExpiresAt.Should().BeOnOrAfter(startedAt.AddMinutes(60));
        result.ExpiresAt.Should().BeOnOrBefore(DateTime.UtcNow.AddMinutes(60));
    }

    [Fact]
    public async Task Handle_WithUnknownRefreshToken_ShouldThrowUnauthorizedAccessExceptionAndNotModifyState()
    {
        // Arrange
        var command = new RotateRefreshTokenCommand("missing-token");
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
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
        var refreshToken = new RefreshToken(
            "revoked-token",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(5));
        refreshToken.Revoke();
        var command = new RotateRefreshTokenCommand(refreshToken.Token);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
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
        var refreshToken = new RefreshToken(
            "valid-refresh-token",
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(refreshToken.Token);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
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
        var refreshToken = new RefreshToken(
            "valid-refresh-token",
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(refreshToken.Token);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
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
        var refreshToken = new RefreshToken(
            "valid-refresh-token",
            user.Id,
            DateTime.UtcNow.AddMinutes(5));
        var command = new RotateRefreshTokenCommand(refreshToken.Token);
        var cancellationToken = CancellationToken.None;

        _refreshTokenRepositoryMock
            .Setup(repository => repository.GetByTokenAsync(command.RefreshToken, cancellationToken))
            .ReturnsAsync(refreshToken);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdAsync(user.Id, cancellationToken))
            .ReturnsAsync(user);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.UpdateAsync(refreshToken, cancellationToken))
            .Returns(Task.CompletedTask);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns("new-access-token");

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns("new-refresh-token");

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
