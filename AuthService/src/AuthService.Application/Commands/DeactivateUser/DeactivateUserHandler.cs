using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.DeleteUser;

public class DeactivateUserHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeactivateUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    public DeactivateUserHandler(IUserRepository userRepository,
        ILogger<DeactivateUserHandler> logger,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} successfully deactivated", request.userId);
    }

}
