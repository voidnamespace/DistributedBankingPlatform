namespace FeeService.Application.Interfaces;

public interface IInboxDispatcher
{
    Task DispatchAsync(
        Guid id,
        string type,
        string payload,
        CancellationToken cancellationToken = default);
}
