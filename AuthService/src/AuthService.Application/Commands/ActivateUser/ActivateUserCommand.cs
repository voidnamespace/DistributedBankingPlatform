using MediatR;
namespace AuthService.Application.Commands.ActivateUser;

public record ActivateUserCommand(Guid UserId) : IRequest;
