using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using FeeService.Infrastructure.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace FeeService.Infrastructure.Messaging.Inbox.Handlers.Users;

public sealed class UserDeactivatedInboxHandler : IInboxMessageHandler<UserDeactivatedIntegrationEvent>
{
    private readonly FeeDbContext _dbContext;

    public UserDeactivatedInboxHandler(FeeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        UserDeactivatedIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        var state = await _dbContext.UserMaintenanceFeeStates
            .SingleOrDefaultAsync(x => x.UserId == message.UserId, cancellationToken);

        if (state is null)
            return;

        _dbContext.UserMaintenanceFeeStates.Remove(state);
    }
}
