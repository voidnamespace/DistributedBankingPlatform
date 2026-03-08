using MediatR;

namespace AccountService.Application.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid AccountId, Guid UserId) : IRequest;

