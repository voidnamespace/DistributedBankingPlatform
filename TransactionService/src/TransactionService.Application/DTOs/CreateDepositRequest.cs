namespace TransactionService.Application.DTOs;

public sealed record CreateDepositRequest(
    string ToAccountNumber, 
    decimal Amount,
    int Currency);
