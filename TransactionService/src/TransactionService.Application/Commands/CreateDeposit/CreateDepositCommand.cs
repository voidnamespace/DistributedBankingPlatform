using MediatR;

namespace TransactionService.Application.Commands.CreateDeposit;

public sealed record CreateDepositCommand(
    string ToAccountNumber,
    decimal Amount,
    int Currency) : IRequest<Guid>;

