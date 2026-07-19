using MediatR;

namespace AuthService.Application.Features.Registration.RegisterUser;

public record RegisterUserCommand (
    string Email,
    string Password
    ) : IRequest<RegisterUserResult>;
