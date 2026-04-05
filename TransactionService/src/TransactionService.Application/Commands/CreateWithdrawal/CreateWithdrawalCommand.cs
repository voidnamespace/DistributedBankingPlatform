using MediatR;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public sealed record CreateWithdrawalCommand(
    Guid InitiatorId,
    string FromAccountNumber,
    decimal Amount,
    int Currency) : IRequest<Guid>;
