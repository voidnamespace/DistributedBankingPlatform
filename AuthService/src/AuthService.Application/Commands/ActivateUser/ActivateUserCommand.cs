using MediatR;
namespace AuthService.Application.Commands.DeleteUser;

public record ActivateUserCommand(Guid userId) : IRequest;
