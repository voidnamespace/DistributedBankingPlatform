using System.Text.Json;
using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Domain.Entities;
using FeeService.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeeService.Infrastructure.Messaging.Inbox;

public sealed class InboxMessageHandler : IInboxMessageHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly FeeDbContext _dbContext;
    private readonly ILogger<InboxMessageHandler> _logger;

    public InboxMessageHandler(
        FeeDbContext dbContext,
        ILogger<InboxMessageHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(
        Guid id,
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        switch (type)
        {
            case "user.created":
                await HandleUserCreatedAsync(payload, cancellationToken);
                break;
            case "user.deleted":
                await HandleUserDeletedAsync(payload, cancellationToken);
                break;
            case "user.activated":
            case "user.deactivated":
                _logger.LogInformation("Inbox message {MessageType} does not require state changes yet", type);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown inbox message type: {type}");
        }
    }

    private async Task HandleUserCreatedAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<UserCreatedIntegrationEvent>(payload, SerializerOptions);

        if (integrationEvent is null)
            throw new InvalidOperationException(
                "user.created payload deserialized null");

        var exists = await _dbContext.UserMaintenanceFeeStates
            .AnyAsync(
                x => x.UserId == integrationEvent.UserId,
                cancellationToken);

        if (exists)
            return;

        var state = UserMaintenanceFeeState.Create(integrationEvent.UserId);

        await _dbContext.UserMaintenanceFeeStates
            .AddAsync(state, cancellationToken);
    }

    private async Task HandleUserDeletedAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        var integrationEvent =
            JsonSerializer.Deserialize<UserDeletedIntegrationEvent>(payload, SerializerOptions);

        if (integrationEvent is null)
            throw new InvalidOperationException(
                "user.deleted payload deserialized null");

        var state = await _dbContext.UserMaintenanceFeeStates
            .SingleOrDefaultAsync(
                x => x.UserId == integrationEvent.UserId,
                cancellationToken);

        if (state is null)
            return;

        _dbContext.UserMaintenanceFeeStates.Remove(state);
    }
}
