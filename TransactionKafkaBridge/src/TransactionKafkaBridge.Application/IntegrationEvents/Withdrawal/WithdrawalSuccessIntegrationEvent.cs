namespace TransactionKafkaBridge.Application.IntegrationEvents.Withdrawal;

public sealed record WithdrawalSuccessIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency);
