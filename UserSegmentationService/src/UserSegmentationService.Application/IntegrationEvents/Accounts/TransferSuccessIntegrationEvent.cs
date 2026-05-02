using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Accounts;

public sealed record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
