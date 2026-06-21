using TransactionService.Application.FraudDetection;
using TransactionService.Infrastructure.Grpc.FraudDetection;
using ApplicationFraudCheckDecision = TransactionService.Application.FraudDetection.FraudCheckDecision;
using GrpcFraudDecision = TransactionService.Infrastructure.Grpc.FraudDetection.FraudDecision;

namespace TransactionService.Infrastructure.FraudDetection;

public sealed class GrpcFraudDetectionClient : IFraudDetectionClient
{
    private readonly FraudCheck.FraudCheckClient _client;

    public GrpcFraudDetectionClient(FraudCheck.FraudCheckClient client)
    {
        _client = client;
    }

    public async Task<FraudCheckResult> CheckTransferAsync(
        FraudCheckRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _client.CheckTransferAsync(
            new CheckTransferRequest
            {
                TransactionId = request.TransactionId.ToString(),
                UserId = request.UserId.ToString(),
                FromAccountNumber = request.FromAccountNumber,
                ToAccountNumber = request.ToAccountNumber,
                Amount = Convert.ToDouble(request.Amount),
                Currency = request.Currency
            },
            cancellationToken: cancellationToken);

        return new FraudCheckResult(
            MapDecision(response.Decision),
            response.RiskScore,
            response.Reasons.ToArray());
    }

    private static ApplicationFraudCheckDecision MapDecision(
        GrpcFraudDecision decision)
    {
        return decision switch
        {
            GrpcFraudDecision.Approved => ApplicationFraudCheckDecision.Approved,
            GrpcFraudDecision.Review => ApplicationFraudCheckDecision.Review,
            GrpcFraudDecision.Rejected => ApplicationFraudCheckDecision.Rejected,
            _ => ApplicationFraudCheckDecision.Rejected
        };
    }
}
