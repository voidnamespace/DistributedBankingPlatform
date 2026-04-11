namespace FeeService.Application.Interfaces;

public interface IInboxMessageHandler
{
    Task HandleAsync(
        Guid id,
        string type,
        string payload,
        CancellationToken cancellationToken = default);
}
