using MediatR;

namespace UserSegmentationService.Application.IntegrationEvents.Accounts;

public sealed record UserAccountsBackfillRequestedIntegrationEvent(
    Guid RequestId,
    DateTime RequestedAt) : INotification;
