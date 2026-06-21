namespace TransactionService.Application.FraudDetection;

public sealed record FraudCheckResult(
    FraudCheckDecision Decision,
    int RiskScore,
    IReadOnlyCollection<string> Reasons);
