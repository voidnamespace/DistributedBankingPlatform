using MediatR;

namespace TransactionService.Application.IntegrationEvents.Deposit;

public sealed record DepositFailedIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
