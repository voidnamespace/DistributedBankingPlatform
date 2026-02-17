using TransactionService.Domain.Enums;
namespace TransactionService.Application.DTOs;

public record CreateTransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency
);
