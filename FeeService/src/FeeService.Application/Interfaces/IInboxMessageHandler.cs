namespace FeeService.Application.Interfaces;

public interface IInboxMessageHandler<in TMessage>
{
    Task HandleAsync(
        TMessage message,
        CancellationToken cancellationToken = default);
}
