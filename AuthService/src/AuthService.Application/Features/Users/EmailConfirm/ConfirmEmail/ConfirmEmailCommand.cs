using MediatR;

namespace AuthService.Application.Features.Users.EmailConfirm.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Token) : IRequest;
