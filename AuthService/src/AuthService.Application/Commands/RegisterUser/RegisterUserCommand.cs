using MediatR;

namespace AuthService.Application.Commands.RegisterUser;

public record RegisterUserCommand (
    string Email,
    string Password
    ) : IRequest<RegisterUserResult>;
