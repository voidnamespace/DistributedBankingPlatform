using MediatR;

namespace AccountService.Application.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid accId) : IRequest;

