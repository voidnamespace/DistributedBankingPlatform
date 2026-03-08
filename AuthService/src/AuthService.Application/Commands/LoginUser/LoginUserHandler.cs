using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LoginUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUserHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LoginUserHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> Handle(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting login for {Email}",
            command.Email);

        if (string.IsNullOrWhiteSpace(command.Password) ||
            string.IsNullOrWhiteSpace(command.Email))
        {
            _logger.LogWarning(
                "Login failed due to missing credentials");

            throw new ArgumentException("Email and password are required.");
        }

        var email = new EmailVO(command.Email);

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed. User not found {Email}",
                email.Value);

            throw new UnauthorizedAccessException("Incorrect email or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning(
                "Login attempt for deactivated user {UserId}",
                user.Id);

            throw new UnauthorizedAccessException("User is deactivated");
        }

        if (!BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash.Hash))
        {
            _logger.LogWarning(
                "Login failed. Invalid password for {Email}",
                email.Value);

            throw new UnauthorizedAccessException("Incorrect email or password");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} successfully logged in",
            user.Id);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            UserId = user.Id,
            Email = user.Email.Value,
            Role = user.Role.ToString()
        };
    }
}