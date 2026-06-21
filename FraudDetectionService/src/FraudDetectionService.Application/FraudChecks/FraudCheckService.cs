namespace FraudDetectionService.Application.FraudChecks;

public sealed class FraudCheckService : IFraudCheckService
{
    public Task<CheckTransferFraudResult> CheckTransferAsync(
        CheckTransferFraudRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Amount >= 500_000)
        {
            return Task.FromResult(new CheckTransferFraudResult(
                FraudCheckDecision.Rejected,
                95,
                ["Transfer amount is too high."]));
        }

        if (request.Amount >= 100_000)
        {
            return Task.FromResult(new CheckTransferFraudResult(
                FraudCheckDecision.Review,
                70,
                ["Transfer amount requires manual review."]));
        }

        return Task.FromResult(new CheckTransferFraudResult(
            FraudCheckDecision.Approved,
            10,
            ["No fraud rules triggered."]));
    }
}
