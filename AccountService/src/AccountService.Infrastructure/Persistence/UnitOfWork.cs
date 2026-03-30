using AccountService.Application.Exceptions;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Infrastructure.Data;
using AccountService.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace AccountService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;
    private readonly IMediator _mediator;
    public UnitOfWork(
        AccountDbContext context,
        IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            var entities = _context.ChangeTracker
           .Entries<Entity>()
           .Where(x => x.Entity.DomainEvents.Any())
           .Select(x => x.Entity)
           .ToList();

            var domainEvents = entities.
                SelectMany(x => x.DomainEvents)
                .ToList();


            foreach (var domainEvent in domainEvents)
            {
                var notificationType =
                        typeof(DomainEventNotification<>)
                            .MakeGenericType(domainEvent.GetType());

                var notification =
                        Activator.CreateInstance(notificationType, domainEvent); 


                await _mediator.Publish((INotification)notification!, ct);
            }


            entities.ForEach(e => e.ClearDomainEvents());



            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("Concurrent update detected");
        }
    }
}
