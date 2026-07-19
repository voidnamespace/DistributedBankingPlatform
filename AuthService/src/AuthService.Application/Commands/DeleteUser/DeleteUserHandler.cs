using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.AccountServiceCalling;
using AuthService.Application.Interfaces.AccountServiceCalling.Contracts;
using AuthService.Application.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDeletionValidator _userValidation;

    public DeleteUserHandler(
        IUserRepository userRepository,
        ILogger<DeleteUserHandler> logger,
        IUnitOfWork unitOfWork,
        IUserDeletionValidator userValidation)
    {
        _userRepository = userRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _userValidation = userValidation;
    }

    public async Task Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to delete user {UserId}",
            command.UserId);

        var user = await _userRepository.GetByIdAsync(
                            command.UserId,
                            cancellationToken);

        if (user == null)
        {
            _logger.LogWarning(
                "Delete failed. User not found {UserId}",
                command.UserId);

            throw new KeyNotFoundException($"User with ID {command.UserId} not found");
        }

        var message = new UserDeletionValidationRequest(
            UserId: command.UserId);

        var validation = await _userValidation.ValidateUserDeletion(message);

        if (!validation.IsAllowed)
        {
            throw new UserDeletionRejectedException(
                "User cannot be deleted while account deletion validation failed.");
        }

        user.Delete();

        await _userRepository.DeleteAsync(
            user,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} successfully deleted",
            command.UserId);
    }
}
