using MediatR;

namespace AccountService.Application.Commands.DeleteAccountEventChain;

public record DeleteAccountsForDeletedUserCommand(Guid UserId) : IRequest;

