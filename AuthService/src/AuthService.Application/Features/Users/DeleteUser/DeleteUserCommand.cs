using MediatR;

namespace AuthService.Application.Features.Users.DeleteUser;

public record DeleteUserCommand(Guid UserId) : IRequest;
