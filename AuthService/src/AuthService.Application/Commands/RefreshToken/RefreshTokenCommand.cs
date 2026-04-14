using MediatR;

namespace AuthService.Application.Commands.MakeRefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;