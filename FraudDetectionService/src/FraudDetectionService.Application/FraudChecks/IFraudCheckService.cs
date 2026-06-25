namespace FraudDetectionService.Application.FraudChecks;

public interface IFraudCheckService
{
    Task<CheckTransferFraudResult> CheckTransferAsync(
        CheckTransferFraudRequest request,
        CancellationToken cancellationToken);
}
