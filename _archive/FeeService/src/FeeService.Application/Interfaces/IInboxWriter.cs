namespace FeeService.Application.Interfaces;

public interface IInboxWriter
{
    Task SaveAsync(
        Guid messageId,
        string type,
        string payload,
        CancellationToken cancellationToken = default);
}
