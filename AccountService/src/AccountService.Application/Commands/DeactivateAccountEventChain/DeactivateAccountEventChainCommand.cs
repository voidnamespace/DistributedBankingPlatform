using MediatR;
namespace AccountService.Application.Commands.DeactivateAccountEventChain;

public record DeactivateAccountEventChainCommand(Guid UserId) : IRequest;