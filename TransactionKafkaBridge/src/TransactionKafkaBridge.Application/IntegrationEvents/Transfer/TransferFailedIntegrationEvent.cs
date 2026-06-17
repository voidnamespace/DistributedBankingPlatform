namespace TransactionKafkaBridge.Application.IntegrationEvents.Transfer;

public sealed record TransferFailedIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency);
