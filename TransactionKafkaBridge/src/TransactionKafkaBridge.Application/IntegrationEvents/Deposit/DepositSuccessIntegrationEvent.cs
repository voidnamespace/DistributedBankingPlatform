namespace TransactionKafkaBridge.Application.IntegrationEvents.Deposit;

public sealed record DepositSuccessIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency);
