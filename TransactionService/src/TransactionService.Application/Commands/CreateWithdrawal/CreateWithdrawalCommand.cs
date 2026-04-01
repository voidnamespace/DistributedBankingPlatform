using MediatR;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public sealed record CreateWithdrawalCommand(
    string AccountNumber,
    decimal Amount,
    int Currency) : IRequest<Guid>;
