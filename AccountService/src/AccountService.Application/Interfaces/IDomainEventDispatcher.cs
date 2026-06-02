namespace AccountService.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(CancellationToken ct);
}
