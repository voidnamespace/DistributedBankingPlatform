using AuthService.Application.DTOs;
using MediatR;
namespace AuthService.Application.Commands.RegisterUser;

public record RegisterUserCommand (
    string Email,
    string Password,
    string ConfirmPassword
    ) : IRequest<RegisterResponse>;
