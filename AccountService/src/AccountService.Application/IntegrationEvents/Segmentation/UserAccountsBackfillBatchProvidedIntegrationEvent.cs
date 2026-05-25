namespace AccountService.Application.IntegrationEvents.Segmentation;

public sealed record UserAccountsBackfillBatchProvidedIntegrationEvent(
    Guid RequestId,
    int BatchNumber,
    bool IsLastBatch,
    DateTime ProvidedAt,
    IReadOnlyList<UserAccountBackfillItem> Accounts);

public sealed record UserAccountBackfillItem(
    Guid UserId,
    Guid AccountId,
    string AccountNumber);
