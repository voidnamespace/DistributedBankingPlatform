using MediatR;

namespace AccountService.Application.Commands.DeleteAccountEventChain;

public record DeleteAccountEventChainCommand(Guid UserId) : IRequest;

