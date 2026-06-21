namespace TransactionService.Application.FraudDetection;

public sealed record FraudCheckRequest(
    Guid TransactionId,
    Guid UserId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    string Currency);
