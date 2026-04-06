namespace TransactionService.Application.DTOs;

public sealed record CreateTransferRequest(
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency
);
