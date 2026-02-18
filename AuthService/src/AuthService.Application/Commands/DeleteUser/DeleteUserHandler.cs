using AuthService.Application.IntegrationEvents;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Messaging;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    public DeleteUserHandler (IUserRepository userRepository, 
        ILogger<DeleteUserHandler> logger,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository; 
        _logger = logger;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }
    public async Task Handle (DeleteUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to delete user {UserId}", request.userId);

        var user = await _userRepository.GetByIdAsync(request.userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {request.userId} not found");
        }

        await _userRepository.DeleteAsync(request.userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishAsync(
            new UserDeletedIntegrationEvent(
                user.Id), 
            "user.deleted",
            cancellationToken);

        _logger.LogInformation("User {UserId} successfully deleted", request.userId);
    }

}
