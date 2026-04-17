using MediatR;

namespace AuthService.Application.Commands.RotateRefreshToken;

public record RotateRefreshTokenCommand(string RefreshToken) : IRequest<RotateRefreshTokenResult>;
