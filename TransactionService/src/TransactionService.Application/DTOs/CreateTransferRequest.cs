using TransactionService.Domain.Enums;
namespace TransactionService.Application.DTOs;

public record CreateTransferRequest(
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    Currency Currency
);
