using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Transfer;

public sealed record TransferCreatedIntegrationEvent(
    Guid InitiatorId,
    Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;

