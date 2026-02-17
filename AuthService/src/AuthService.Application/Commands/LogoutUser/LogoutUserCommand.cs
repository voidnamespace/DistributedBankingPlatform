using MediatR;
namespace AuthService.Application.Commands.LogoutUser;

public record LogoutUserCommand(Guid userId) : IRequest;
