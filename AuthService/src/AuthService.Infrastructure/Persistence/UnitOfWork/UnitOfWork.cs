using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using MediatR;
namespace AuthService.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    private readonly IMediator _mediator;

    public UnitOfWork(
        AuthDbContext context,
        IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            var notificationType =
                typeof(DomainEventNotification<>)
                    .MakeGenericType(domainEvent.GetType());

            var notification =
                Activator.CreateInstance(notificationType, domainEvent);

            await _mediator.Publish((INotification)notification!, cancellationToken);
        }
    }
}