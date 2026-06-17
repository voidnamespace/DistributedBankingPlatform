namespace TransactionKafkaBridge.Application.IntegrationEvents.Transfer;

public sealed record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency);
