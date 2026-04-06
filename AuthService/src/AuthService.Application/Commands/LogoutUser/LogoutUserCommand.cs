using MediatR;
namespace AuthService.Application.Commands.LogoutUser;

public record LogoutUserCommand(Guid UserId) : IRequest;
