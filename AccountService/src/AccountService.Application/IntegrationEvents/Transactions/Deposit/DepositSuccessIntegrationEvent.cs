using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Deposit;

public sealed record DepositSuccessIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
