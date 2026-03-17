using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.DeleteUser;

public class ActivateUserHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ActivateUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateUserHandler(
        IUserRepository userRepository,
        ILogger<ActivateUserHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to activate user {UserId}",
            request.userId);

        var user = await _userRepository.GetByIdAsync(
            request.userId,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Activation failed. User not found {UserId}",
                request.userId);

            throw new KeyNotFoundException($"User with ID {request.userId} not found");
        }

        user.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} successfully activated",
            request.userId);
    }
}