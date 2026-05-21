using MediatR;
using Microsoft.EntityFrameworkCore;
using UserSegmentationService.Application.Common.DomainEvents;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Events;
using UserSegmentationService.Infrastructure.Persistence.Database;

namespace UserSegmentationService.Infrastructure.Persistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly SegmentationDbContext _dbContext;
    private readonly IMediator _mediator;

    public UnitOfWork(
        SegmentationDbContext dbContext,
        IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var entitiesWithDomainEvents = _dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToArray();

        var domainEvents = entitiesWithDomainEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToArray();

        foreach (var entity in entitiesWithDomainEvents)
        {
            entity.ClearDomainEvents();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in domainEvents)
        {
            await PublishDomainEventAsync(domainEvent, cancellationToken);
        }
    }
    
    private Task PublishDomainEventAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var domainEventType = domainEvent.GetType();

        var openGenericNotificationType = typeof(DomainEventNotification<>);

        var closedGenericNotificationType = openGenericNotificationType.MakeGenericType(domainEventType);

        var notification = Activator.CreateInstance(
            closedGenericNotificationType,
            domainEvent);


        //var notificationType =
        //            typeof(DomainEventNotification<>)
        //                .MakeGenericType(domainEvent.GetType());

        //var notification =
        //    Activator.CreateInstance(notificationType, domainEvent);



        return _mediator.Publish(
            (INotification)notification!,
            cancellationToken);
    }
}
