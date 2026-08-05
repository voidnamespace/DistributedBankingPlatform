namespace TransactionService.API.Contracts;

public sealed record CreateWithdrawalRequest(
    string AccountNumber,
    decimal Amount,
    int Currency);
