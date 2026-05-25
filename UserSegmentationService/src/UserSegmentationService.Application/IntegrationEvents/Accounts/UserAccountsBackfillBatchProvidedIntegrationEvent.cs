using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Accounts;

public sealed record UserAccountsBackfillBatchProvidedIntegrationEvent(
    Guid RequestId,
    int BatchNumber,
    bool IsLastBatch,
    DateTime ProvidedAt,
    IReadOnlyList<UserAccountBackfillItem> Accounts) : INotification;

public sealed record UserAccountBackfillItem(
    Guid UserId,
    Guid AccountId,
    string AccountNumber);
