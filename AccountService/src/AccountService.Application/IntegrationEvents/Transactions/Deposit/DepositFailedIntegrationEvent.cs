using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Deposit;

public sealed record DepositFailedIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;

