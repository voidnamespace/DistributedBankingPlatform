using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Application.Common.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AuthDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        AuthDbContext context,
        IMediator mediator,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
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

        if (domainEvents.Count > 0)
        {
            _logger.LogInformation(
                "Dispatching {Count} domain events",
                domainEvents.Count);
        }

        foreach (var domainEvent in domainEvents)
        {
            var notificationType =
                typeof(DomainEventNotification<>)
                    .MakeGenericType(domainEvent.GetType());

            var notification =
                Activator.CreateInstance(notificationType, domainEvent);

            _logger.LogDebug(
                "Publishing domain event {EventType}",
                domainEvent.GetType().Name);

            await _mediator.Publish(
                (INotification)notification!,
                cancellationToken);
        }

        entities.ForEach(e => e.ClearDomainEvents());

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("UnitOfWork SaveChangesAsync completed");
    }
}