using MediatR;

namespace AccountService.Application.Commands.ActivateAccount;

public record ActivateAccountCommand(Guid AccountId) : IRequest;