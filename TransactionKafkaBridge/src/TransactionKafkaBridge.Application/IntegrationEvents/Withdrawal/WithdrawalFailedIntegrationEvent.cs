namespace TransactionKafkaBridge.Application.IntegrationEvents.Withdrawal;

public sealed record WithdrawalFailedIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency);

