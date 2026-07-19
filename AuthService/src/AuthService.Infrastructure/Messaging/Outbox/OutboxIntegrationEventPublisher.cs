using AuthService.Application.Abstractions.Messaging;
using AuthService.Infrastructure.Messaging.IntegrationEvents;
using AuthService.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuthService.Infrastructure.Messaging.Outbox;

public sealed class OutboxIntegrationEventPublisher
    : IIntegrationEventPublisher
{
    private readonly AuthDbContext _context;
    private readonly ILogger<OutboxIntegrationEventPublisher> _logger;

    public OutboxIntegrationEventPublisher(
        AuthDbContext context,
        ILogger<OutboxIntegrationEventPublisher> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task PublishAsync<TIntegrationEvent>(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        string payload;

        try
        {
            payload = JsonSerializer.Serialize(integrationEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to serialize integration event {EventType}",
                typeof(TIntegrationEvent).Name);

            throw;
        }

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),

            Type = IntegrationEventMap.GetName(typeof(TIntegrationEvent)),

            Payload = payload,

            CreatedAt = DateTime.UtcNow,

            AttemptCount = 0
        };

        _context.OutboxMessages.Add(message);

        _logger.LogInformation(
            "Outbox message queued. Type={Type} Id={Id}",
            message.Type,
            message.Id);

        return Task.CompletedTask;
    }
}
