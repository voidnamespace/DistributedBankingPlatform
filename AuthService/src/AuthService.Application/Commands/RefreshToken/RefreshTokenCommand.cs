using AuthService.Application.DTOs;
using MediatR;
namespace AuthService.Application.Commands.MakeRefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResponse>;
