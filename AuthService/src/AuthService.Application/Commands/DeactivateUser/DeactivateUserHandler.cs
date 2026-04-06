using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.DeactivateUser;

public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeactivateUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public DeactivateUserHandler(
        IUserRepository userRepository,
        ILogger<DeactivateUserHandler> logger,
        IUnitOfWork unitOfWork,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeactivateUserCommand started for {UserId}",
            command.UserId);

        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "DeactivateUserCommand failed: user not found {UserId}",
                command.UserId);

            throw new KeyNotFoundException($"User with ID {command.UserId} not found");
        }

        user.Deactivate();

        await _refreshTokenRepository.RevokeAllUserTokensAsync(
            command.UserId,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeactivateUserCommand completed for {UserId}",
            command.UserId);
    }
}
