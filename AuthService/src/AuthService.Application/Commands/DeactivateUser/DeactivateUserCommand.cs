using MediatR;
namespace AuthService.Application.Commands.DeleteUser;

public record DeactivateUserCommand(Guid userId) : IRequest;
