using TransactionService.Domain.Enums;
namespace TransactionService.Application.DTOs;

public record CreateTransferRequest(
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency
);
