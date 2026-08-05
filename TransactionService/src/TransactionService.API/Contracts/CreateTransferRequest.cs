namespace TransactionService.API.Contracts;

public sealed record CreateTransferRequest(
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency
);
