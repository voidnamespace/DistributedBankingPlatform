using MediatR;

namespace TransactionService.Application.IntegrationEvents.Deposit;

public sealed record DepositCreatedIntegrationEvent(
    Guid TransactionId,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
