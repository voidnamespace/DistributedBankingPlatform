namespace TransactionService.Application.DTOs;

public sealed record CreateWithdrawalRequest(
    string AccountNumber,
    decimal Amount,
    int Currency);
