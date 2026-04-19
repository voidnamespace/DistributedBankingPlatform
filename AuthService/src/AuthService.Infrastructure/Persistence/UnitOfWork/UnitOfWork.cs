using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using AuthService.Application.Common.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        try
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            if (domainEvents.Count > 0)
            {
                _logger.LogInformation(
                    "Dispatching {Count} domain events after persisting state",
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

            if (domainEvents.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            entities.ForEach(e => e.ClearDomainEvents());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "UnitOfWork SaveChangesAsync failed while saving transactional changes");

            throw;
        }

        _logger.LogDebug("UnitOfWork SaveChangesAsync completed");
    }
}
