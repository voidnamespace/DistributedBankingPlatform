using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.Commands.CreateTransfer;

public record CreateTransferCommand(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency
) : IRequest<Guid>;
