using AccountService.Application.Exceptions;
using AccountService.Application.Interfaces;
using AccountService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountService.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(
        AccountDbContext context,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _domainEventDispatcher.DispatchAsync(ct);

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
