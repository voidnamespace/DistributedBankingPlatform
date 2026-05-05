using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Domain.Entities;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Inbox.Handlers.Users;

public sealed class UserCreatedInboxHandler : IInboxMessageHandler<UserCreatedIntegrationEvent>
{
    private readonly FeeDbContext _dbContext;

    public UserCreatedInboxHandler(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        UserCreatedIntegrationEvent message,
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
