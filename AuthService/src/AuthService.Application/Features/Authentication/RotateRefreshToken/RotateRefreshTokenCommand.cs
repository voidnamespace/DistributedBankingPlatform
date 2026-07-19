using MediatR;

namespace AuthService.Application.Features.Authentication.RotateRefreshToken;

public record RotateRefreshTokenCommand(string RefreshToken) : IRequest<RotateRefreshTokenResult>;
