using MediatR;

namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
