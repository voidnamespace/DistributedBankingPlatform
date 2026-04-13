using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Domain.Entities;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Messaging.Inbox.Handlers.Users;

public sealed class UserActivatedInboxHandler : IInboxMessageHandler<UserActivatedIntegrationEvent>
{
    private readonly FeeDbContext _dbContext;

    public UserActivatedInboxHandler(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        UserActivatedIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        var exists = await _dbContext.UserMaintenanceFeeStates
            .AnyAsync(x => x.UserId == message.UserId, cancellationToken);

        if (exists)
            return;

        var state = UserMaintenanceFeeState.Create(message.UserId);
        await _dbContext.UserMaintenanceFeeStates.AddAsync(state, cancellationToken);
    }
}
