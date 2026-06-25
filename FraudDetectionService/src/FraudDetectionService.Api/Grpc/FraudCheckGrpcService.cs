namespace FraudDetectionService.Api.Grpc
{
    public sealed class FraudCheckGrpcService(
        FraudDetectionService.Application.FraudChecks.IFraudCheckService fraudCheckService) : FraudCheck.FraudCheckBase
    {
        public override async Task<CheckTransferResponse> CheckTransfer(
            CheckTransferRequest request,
            global::Grpc.Core.ServerCallContext context)
        {
            var result = await fraudCheckService.CheckTransferAsync(
                new FraudDetectionService.Application.FraudChecks.CheckTransferFraudRequest(
                    request.TransactionId,
                    request.UserId,
                    request.FromAccountNumber,
                    request.ToAccountNumber,
                    Convert.ToDecimal(request.Amount),
                    request.Currency),
                context.CancellationToken);

            var response = new CheckTransferResponse
            {
                Decision = MapDecision(result.Decision),
                RiskScore = result.RiskScore
            };
            response.Reasons.AddRange(result.Reasons);

            return response;
        }

        private static FraudDecision MapDecision(
            FraudDetectionService.Application.FraudChecks.FraudCheckDecision decision)
        {
            return decision switch
            {
                FraudDetectionService.Application.FraudChecks.FraudCheckDecision.Approved => FraudDecision.Approved,
                FraudDetectionService.Application.FraudChecks.FraudCheckDecision.Review => FraudDecision.Review,
                FraudDetectionService.Application.FraudChecks.FraudCheckDecision.Rejected => FraudDecision.Rejected,
                _ => (FraudDecision)0
            };
        }
    }
}
