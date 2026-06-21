namespace FraudDetectionService.Application.FraudChecks;

public sealed record CheckTransferFraudResult(
    FraudCheckDecision Decision,
    int RiskScore,
    IReadOnlyCollection<string> Reasons);
