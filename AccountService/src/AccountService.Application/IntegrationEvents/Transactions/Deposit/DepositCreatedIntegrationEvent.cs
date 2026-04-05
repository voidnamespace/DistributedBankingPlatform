using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Deposit;

public sealed record DepositCreatedIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
