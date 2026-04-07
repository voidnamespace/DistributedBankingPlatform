using MediatR;
using AuthService.Application.DTOs;

namespace AuthService.Application.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<IEnumerable<UserDTO>>;
