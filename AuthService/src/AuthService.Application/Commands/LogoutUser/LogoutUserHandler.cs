using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.LogoutUser;

public class LogoutUserHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LogoutUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<LogoutUserHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        LogoutUserCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "LogoutUserCommand started {UserId}",
            command.UserId);

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
