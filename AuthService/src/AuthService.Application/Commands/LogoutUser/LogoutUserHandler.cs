using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.LogoutUser;

public class LogoutUserHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<LogoutUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserHandler(IRefreshTokenRepository refreshTokenRepository,
        ILogger<LogoutUserHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle (LogoutUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to logout user {UserId}", request.userId);

        await _refreshTokenRepository.RevokeAllUserTokensAsync(request.userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully logged out", request.userId);
    }
}
