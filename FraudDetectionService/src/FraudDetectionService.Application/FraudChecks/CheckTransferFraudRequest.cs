namespace FraudDetectionService.Application.FraudChecks;

public sealed record CheckTransferFraudRequest(
    string TransactionId,
    string UserId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    string Currency);
