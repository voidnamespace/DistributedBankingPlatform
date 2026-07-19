using MediatR;
namespace AuthService.Application.Features.Users.ActivateUser;

public record ActivateUserCommand(Guid UserId) : IRequest;
