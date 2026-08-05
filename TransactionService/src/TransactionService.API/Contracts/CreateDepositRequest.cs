namespace TransactionService.API.Contracts;

public sealed record CreateDepositRequest(
    string ToAccountNumber, 
    decimal Amount,
    int Currency);
