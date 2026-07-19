using MediatR;
namespace AuthService.Application.Features.Users.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<UserResult>>;
