using MediatR;

namespace AuthService.Application.Features.Users.EmailConfirm.SendEmailConfirmation;

public sealed record SendEmailConfirmationCommand(Guid UserId) : IRequest;
