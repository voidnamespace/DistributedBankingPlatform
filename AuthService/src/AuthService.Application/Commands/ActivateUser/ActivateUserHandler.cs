using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.ActivateUser;

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

    public async Task Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ActivateUserCommand started for {UserId}",
            command.UserId);

        var user = await _userRepository.GetByIdAsync(
            command.UserId,
            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "ActivateUserCommand failed: user not found {UserId}",
                command.UserId);

            throw new KeyNotFoundException($"User with ID {command.UserId} not found");
        }

        user.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "ActivateUserCommand completed for {UserId}",
            command.UserId);
    }
}
