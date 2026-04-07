using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionService.Application.Common;
using TransactionService.Application.Exceptions;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Persistence.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly TransactionDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        TransactionDbContext context,
        IMediator mediator,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        _logger.LogInformation("UnitOfWork SaveChangesAsync started");

        try
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
                    "UnitOfWork dispatching domain events: Count {Count}",
                    domainEvents.Count);
            }

            foreach (var domainEvent in domainEvents)
            {
                var notificationType =
                    typeof(DomainEventNotification<>)
                        .MakeGenericType(domainEvent.GetType());

                var notification =
                    Activator.CreateInstance(
                        notificationType,
                        domainEvent);

                _logger.LogInformation(
                    "Publishing domain event: Type {DomainEventType}",
                    domainEvent.GetType().Name);

                await _mediator.Publish(
                    (INotification)notification!,
                    ct);
            }

            entities.ForEach(e => e.ClearDomainEvents());

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "UnitOfWork SaveChangesAsync committed successfully");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "UnitOfWork concurrency conflict detected");

            throw new ConcurrencyException(
                "Concurrent update detected");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "UnitOfWork SaveChangesAsync failed");

            throw;
        }
    }
}
