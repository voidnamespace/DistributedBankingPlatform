using MediatR;
namespace AuthService.Application.Features.Users.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest;
