using MediatR;

namespace TransactionService.Application.IntegrationEvents.Transfer;

public sealed record TransferCreatedIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
