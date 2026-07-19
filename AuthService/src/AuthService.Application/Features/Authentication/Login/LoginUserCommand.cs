using MediatR;

namespace AuthService.Application.Features.Authentication.Login;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginUserResult>;
