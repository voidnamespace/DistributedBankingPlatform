using Microsoft.Extensions.Logging;
using TransactionService.Application.Interfaces;
namespace TransactionService.Infrastructure.Messaging;

public class FakeEventBus : IEventBus
{
    private readonly ILogger<FakeEventBus> _logger;

    public FakeEventBus(ILogger<FakeEventBus> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T @event)
    {
        _logger.LogInformation(
            "FAKE EVENT PUBLISHED: {EventType}",
            typeof(T).Name);

        return Task.CompletedTask;
    }
}
