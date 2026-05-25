using MediatR;

namespace AccountService.Application.IntegrationEvents.Segmentation;

public sealed record UserAccountsBackfillRequestedIntegrationEvent(
    Guid RequestId,
    DateTime RequestedAt) : INotification;

