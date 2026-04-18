using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.RotateRefreshToken;

public class RotateRefreshTokenHandler : IRequestHandler<RotateRefreshTokenCommand, RotateRefreshTokenResult>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<RotateRefreshTokenHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RotateRefreshTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<RotateRefreshTokenHandler> logger,
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

    public async Task<RotateRefreshTokenResult> Handle(
        RotateRefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("RefreshTokenCommand started");

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token request failed: token not found");
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        _logger.LogInformation(
            "Refresh token validated for user {UserId}",
            refreshToken.UserId);

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

        refreshToken.Revoke();

        _logger.LogInformation(
            "Refresh token revoked for user {UserId}",
            user.Id);

        await _refreshTokenRepository.UpdateAsync(
            refreshToken,
            cancellationToken);


        var newAccessToken = _jwtService.GenerateAccessToken(user);

        _logger.LogInformation(
            "Access token generated via refresh flow for user {UserId}",
            user.Id);

        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken(
            newRefreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(7));

        _logger.LogInformation(
            "New refresh token created for user {UserId}",
            user.Id);

        await _refreshTokenRepository.CreateAsync(
            newRefreshToken,
            cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (IsConcurrencyConflict(ex))
        {
            _logger.LogWarning(
                ex,
                "Refresh token request failed due to concurrent reuse attempt for user {UserId}",
                user.Id);

            throw new UnauthorizedAccessException("Refresh token is invalid or revoked");
        }

        _logger.LogInformation(
            "RefreshTokenCommand completed {UserId}",
            user.Id);

        return new RotateRefreshTokenResult
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };
    }

    private static bool IsConcurrencyConflict(Exception exception)
    {
        return exception.GetType().FullName == "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException";
    }
}
