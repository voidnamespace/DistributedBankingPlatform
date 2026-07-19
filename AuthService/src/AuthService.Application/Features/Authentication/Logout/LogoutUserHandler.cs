using AuthService.Application.Abstractions.Authentication;
using AuthService.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Features.Authentication.Logout;

public class LogoutUserHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LogoutUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenBlacklistStore _tokenBlacklistStore;

    public LogoutUserHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LogoutUserHandler> logger,
        IUnitOfWork unitOfWork,
        ITokenBlacklistStore tokenBlacklistStore)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _tokenBlacklistStore = tokenBlacklistStore;
    }

    public async Task Handle(
        LogoutUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "LogoutUserCommand started {UserId}",
            command.UserId);

        var expiresIn = command.AccessTokenExpiresAt - DateTime.UtcNow;

        if (expiresIn > TimeSpan.Zero) 
        {
            await _tokenBlacklistStore.BlacklistAsync(
                command.AccessTokenId,
                expiresIn,
                cancellationToken);
        }

        await _refreshTokenRepository.RevokeAllUserTokensAsync(
            command.UserId,
            cancellationToken);

        _logger.LogInformation(
            "All refresh tokens revoked {UserId}",
            command.UserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "LogoutUserCommand completed {UserId}",
            command.UserId);
    }
}
