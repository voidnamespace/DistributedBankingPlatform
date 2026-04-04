using MediatR;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public sealed record CreateWithdrawalCommand(
    string FromAccountNumber,
    decimal Amount,
    int Currency) : IRequest<Guid>;
