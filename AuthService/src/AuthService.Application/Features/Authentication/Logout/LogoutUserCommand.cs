using MediatR;

namespace AuthService.Application.Features.Authentication.Logout;

public record LogoutUserCommand(
    Guid UserId,
    string AccessTokenId,
    DateTime AccessTokenExpiresAt) : IRequest;
