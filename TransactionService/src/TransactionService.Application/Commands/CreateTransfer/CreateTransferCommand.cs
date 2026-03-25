using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.Commands.CreateTransfer;

public record CreateTransferCommand(
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency
) : IRequest<Guid>;
