namespace TransactionService.Application.FraudDetection;

public interface IFraudDetectionClient
{
    Task<FraudCheckResult> CheckTransferAsync(
        FraudCheckRequest request,
        CancellationToken cancellationToken);
}
