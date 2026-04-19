using AuthService.Application.Commands.LoginUser;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AuthService.Application.Tests.Commands.LoginUser;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokensAndPersistRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();
        var command = new LoginUserCommand("alice@example.com", "SecurePassword123");
        var cancellationToken = CancellationToken.None;
        RefreshToken? createdRefreshToken = null;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<EmailVO>(email => email.Value == command.Email), cancellationToken))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(user))
            .Returns("access-token");

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns("refresh-token");

        _refreshTokenRepositoryMock
            .Setup(repository => repository.CreateAsync(It.IsAny<RefreshToken>(), cancellationToken))
            .Callback<RefreshToken, CancellationToken>((refreshToken, _) => createdRefreshToken = refreshToken)
            .ReturnsAsync((RefreshToken refreshToken, CancellationToken _) => refreshToken);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var startedAt = DateTime.UtcNow;

        // Act
        var result = await handler.Handle(command, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(
            repository => repository.GetByEmailAsync(It.Is<EmailVO>(email => email.Value == command.Email), cancellationToken),
            Times.Once);
        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), cancellationToken),
            Times.Once);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(cancellationToken),
            Times.Once);

        createdRefreshToken.Should().NotBeNull();
        createdRefreshToken!.UserId.Should().Be(user.Id);
        createdRefreshToken.Token.Should().Be("refresh-token");
        createdRefreshToken.IsRevoked.Should().BeFalse();

        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email.Value);
        result.Role.Should().Be(user.Role.ToString());
        result.ExpiresAt.Should().BeOnOrAfter(startedAt.AddMinutes(15));
        result.ExpiresAt.Should().BeOnOrBefore(DateTime.UtcNow.AddMinutes(15));
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ShouldThrowUnauthorizedAccessExceptionAndNotPersistRefreshToken()
    {
        // Arrange
        var command = new LoginUserCommand("alice@example.com", "SecurePassword123");
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<EmailVO>(email => email.Value == command.Email), cancellationToken))
            .ReturnsAsync((User?)null);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Incorrect email or password");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithDeactivatedUser_ShouldThrowUnauthorizedAccessExceptionAndNotPersistRefreshToken()
    {
        // Arrange
        var user = CreateInactiveUser();
        var command = new LoginUserCommand("alice@example.com", "SecurePassword123");
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<EmailVO>(email => email.Value == command.Email), cancellationToken))
            .ReturnsAsync(user);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User is deactivated");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowUnauthorizedAccessExceptionAndNotPersistRefreshToken()
    {
        // Arrange
        var user = CreateActiveUser();
        var command = new LoginUserCommand("alice@example.com", "WrongPassword123");
        var cancellationToken = CancellationToken.None;

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailAsync(It.Is<EmailVO>(email => email.Value == command.Email), cancellationToken))
            .ReturnsAsync(user);

        var handler = CreateHandler();

        // Act
        Func<Task> act = async () => await handler.Handle(command, cancellationToken);

        // Assert
        await act.Should()
            .ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Incorrect email or password");

        _refreshTokenRepositoryMock.Verify(
            repository => repository.CreateAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private LoginUserHandler CreateHandler()
    {
        return new LoginUserHandler(
            _userRepositoryMock.Object,
            _jwtServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            NullLogger<LoginUserHandler>.Instance,
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
