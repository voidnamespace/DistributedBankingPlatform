using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.MakeRefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RefreshTokenHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RefreshTokenHandler> logger,
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshTokenResponse> Handle(
        RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to refresh access token");

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            _logger.LogWarning("Refresh token request failed: token is empty");
            throw new ArgumentException("Refresh token is required");
        }

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token request failed: token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (!refreshToken.IsActive())
        {
            _logger.LogWarning(
                "Refresh token request failed: token revoked or expired for user {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedAccessException("Refresh token is invalid or revoked");
        }

        var user = await _userRepository.GetByIdAsync(
            refreshToken.UserId,
            cancellationToken);

        if (user == null || !user.IsActive)
        {
            _logger.LogWarning(
                "Refresh token request failed: user not found or inactive {UserId}",
                refreshToken.UserId);

            throw new UnauthorizedAccessException("User not found or inactive");
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        await _refreshTokenRepository.UpdateAsync(
            refreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(
            newRefreshToken,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Access token successfully refreshed for user {UserId}",
            user.Id);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}