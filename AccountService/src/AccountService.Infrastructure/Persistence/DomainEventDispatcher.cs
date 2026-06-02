using AccountService.Application.Common;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Infrastructure.Data;
using MediatR;

namespace AccountService.Infrastructure.Persistence;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly AccountDbContext _context;
    private readonly IMediator _mediator;

    public DomainEventDispatcher(
        AccountDbContext context,
        IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task DispatchAsync(CancellationToken ct)
    {
        while (true)
        {
            var entities = _context.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            var domainEvents = entities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            if (domainEvents.Count == 0)
                return;

            entities.ForEach(x => x.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                var notificationType =
                    typeof(DomainEventNotification<>)
                        .MakeGenericType(domainEvent.GetType());

                var notification =
                    Activator.CreateInstance(
                        notificationType,
                        domainEvent);

                await _mediator.Publish(
                    (INotification)notification!,
                    ct);
            }
        }
    }
}
