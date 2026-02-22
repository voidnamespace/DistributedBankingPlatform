using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Events;
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
        await _context.SaveChangesAsync(cancellationToken);

        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(
                new DomainEventNotification<IDomainEvent>(domainEvent),
                cancellationToken);
        }
    }
}
