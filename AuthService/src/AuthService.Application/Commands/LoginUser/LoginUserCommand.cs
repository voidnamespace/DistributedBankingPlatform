using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<LoginResponse>;
