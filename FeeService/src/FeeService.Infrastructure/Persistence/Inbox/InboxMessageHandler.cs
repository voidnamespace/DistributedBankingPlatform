using System.Text.Json;
using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Domain.Entities;
using FeeService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Persistence.Inbox;

public sealed class InboxMessageHandler : IInboxMessageHandler
{
    private readonly FeeDbContext _dbContext;

    public InboxMessageHandler(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
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
            JsonSerializer.Deserialize<UserCreatedIntegrationEvent>(payload);

        if (integrationEvent is null)
            throw new InvalidOperationException(
                "user.created payload deserialized null");

        var exists =
            await _dbContext.UserMaintenanceFeeStates
                .AnyAsync(
                    x => x.UserId == integrationEvent.UserId,
                    cancellationToken);

        if (exists)
            return;

        var state =
            UserMaintenanceFeeState.Create(
                integrationEvent.UserId);

        await _dbContext.UserMaintenanceFeeStates
            .AddAsync(state, cancellationToken);

    }
}
