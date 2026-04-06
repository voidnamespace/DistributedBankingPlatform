using MediatR;
namespace AuthService.Application.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest;
