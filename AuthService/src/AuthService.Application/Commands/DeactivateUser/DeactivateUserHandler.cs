using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.DeleteUser;

public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeactivateUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    public DeactivateUserHandler(IUserRepository userRepository,
        ILogger<DeactivateUserHandler> logger,
        IUnitOfWork unitOfWork,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _refreshTokenRepository = refreshTokenRepository;
    }
    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to deactivate user {UserId}", request.userId);

        var user = await _userRepository.GetByIdAsync(request.userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.userId} not found");
        }

        await _userRepository.DeactivateAsync(request.userId, cancellationToken);
        await _refreshTokenRepository.RevokeAllUserTokensAsync(request.userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User {UserId} successfully deactivated", request.userId);
    }

}
