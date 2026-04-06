using MediatR;
namespace AccountService.Application.Commands.DeactivateAccountEventChain;

public record DeactivateAccountsForDeactivatedUserCommand(Guid UserId) : IRequest;