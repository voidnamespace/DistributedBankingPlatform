using MediatR;

namespace TransactionService.Application.IntegrationEvents.Withdrawal;

public sealed record WithdrawalCreatedIntegrationEvent(
    Guid InitiatorId,
    Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency) : INotification;
