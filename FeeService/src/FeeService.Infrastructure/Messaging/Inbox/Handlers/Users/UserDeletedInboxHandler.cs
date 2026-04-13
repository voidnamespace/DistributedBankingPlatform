using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Messaging.Inbox.Handlers.Users;

public sealed class UserDeletedInboxHandler : IInboxMessageHandler<UserDeletedIntegrationEvent>
{
    private readonly FeeDbContext _dbContext;

    public UserDeletedInboxHandler(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        UserDeletedIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.UserMaintenanceFeeStates
            .SingleOrDefaultAsync(x => x.UserId == message.UserId, cancellationToken);

        if (state is null)
            return;

        _dbContext.UserMaintenanceFeeStates.Remove(state);
    }
}
