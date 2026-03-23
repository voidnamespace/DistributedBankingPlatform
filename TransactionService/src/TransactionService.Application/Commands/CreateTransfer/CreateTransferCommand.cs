using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.Commands.CreateTransfer;

public record CreateTransferCommand(
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    Currency Currency
) : IRequest<Guid>;
