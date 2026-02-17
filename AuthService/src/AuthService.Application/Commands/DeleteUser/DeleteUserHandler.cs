using AuthService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AuthService.Application.Commands.DeleteUser;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<DeleteUserHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;    
    public DeleteUserHandler (IUserRepository userRepository, ILogger<DeleteUserHandler> logger, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository; 
        _logger = logger;
        _unitOfWork = unitOfWork;
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

        _logger.LogInformation("User {UserId} successfully deleted", request.userId);
    }

}
