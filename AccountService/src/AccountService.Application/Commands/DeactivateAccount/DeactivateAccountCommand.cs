using MediatR;
namespace AccountService.Application.Commands.DeactivateAccount;

public record DeactivateAccountCommand(Guid AccountId) : IRequest;
