using AccountService.Application.Exceptions;
using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Infrastructure.Data;
using AccountService.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        AccountDbContext context,
        IMediator mediator,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        var entities = _context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        if (domainEvents.Count > 0)
        {
            _logger.LogInformation(
                "Dispatching {Count} domain events before committing transaction",
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
                ct);
        }

        entities.ForEach(e => e.ClearDomainEvents());

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "Concurrency conflict detected while saving AccountService changes");

            throw new ConcurrencyException(
                "Concurrent update detected");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "UnitOfWork SaveChangesAsync failed while saving to database");

            throw;
        }

        _logger.LogDebug("UnitOfWork SaveChangesAsync completed");
    }
}
