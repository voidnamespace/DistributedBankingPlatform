using System.Text.Json;
using MediatR;
using UserSegmentationService.Application.IntegrationEvents.Accounts;

namespace UserSegmentationService.Infrastructure.Inbox;

internal class InboxDispatcher
{
    private readonly IMediator _mediator;

    public InboxDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(
        InboxMessage message,
        CancellationToken cancellationToken = default)
    {
        switch (message.Type)
        {
            case "account.created":
                await HandleAccountCreatedAsync(message.Payload, cancellationToken);
                return;

            case "transfer.success":
                await HandleTransferSuccessAsync(message.Payload, cancellationToken);
                return;

            default:
                throw new InvalidOperationException(
                    $"Inbox message type '{message.Type}' is not supported.");
        }
    }

    private async Task HandleAccountCreatedAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<AccountCreatedIntegrationEvent>(payload)
            ?? throw new InvalidOperationException("Account created payload is empty or invalid.");

        await _mediator.Publish(integrationEvent, cancellationToken);
    }

    private async Task HandleTransferSuccessAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        var integrationEvent = JsonSerializer.Deserialize<TransferSuccessIntegrationEvent>(payload)
            ?? throw new InvalidOperationException("Transfer success payload is empty or invalid.");

        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
