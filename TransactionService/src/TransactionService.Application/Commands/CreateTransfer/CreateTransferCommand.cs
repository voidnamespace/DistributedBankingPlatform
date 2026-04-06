using MediatR;

namespace TransactionService.Application.Commands.CreateTransfer;

public record CreateTransferCommand(
    Guid InitiatorId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency
) : IRequest<Guid>;
