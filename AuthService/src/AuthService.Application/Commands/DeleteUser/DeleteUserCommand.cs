using MediatR;

namespace AuthService.Application.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest;
