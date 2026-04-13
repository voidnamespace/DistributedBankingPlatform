using System.Text.Json;
using FeeService.Application.IntegrationEvents.Users;
using FeeService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FeeService.Infrastructure.Messaging.Inbox;

public sealed class InboxDispatcher : IInboxDispatcher
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly IReadOnlyDictionary<string, Type> MessageTypes =
        new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["user.created"] = typeof(UserCreatedIntegrationEvent),
            ["user.deleted"] = typeof(UserDeletedIntegrationEvent),
            ["user.activated"] = typeof(UserActivatedIntegrationEvent),
            ["user.deactivated"] = typeof(UserDeactivatedIntegrationEvent)
        };

    private readonly IServiceProvider _serviceProvider;

    public InboxDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task DispatchAsync(
        Guid id,
        string type,
        string payload,
        CancellationToken cancellationToken = default)
    {
        if (!MessageTypes.TryGetValue(type, out var messageType))
            throw new InvalidOperationException($"Unknown inbox message type: {type}");

        return DispatchInternalAsync(messageType, type, payload, cancellationToken);
    }

    private async Task DispatchInternalAsync(
        Type messageType,
        string type,
        string payload,
        CancellationToken cancellationToken)
    {
        var message = JsonSerializer.Deserialize(payload, messageType, SerializerOptions)
            ?? throw new InvalidOperationException($"{type} payload deserialized null");

        var handlerType = typeof(IInboxMessageHandler<>).MakeGenericType(messageType);
        var handleMethod = handlerType.GetMethod(nameof(IInboxMessageHandler<object>.HandleAsync))
            ?? throw new InvalidOperationException(
                $"Inbox handler method not found for message type {messageType.Name}");

        var handler = _serviceProvider.GetRequiredService(handlerType);
        var task = handleMethod.Invoke(handler, [message, cancellationToken]) as Task
            ?? throw new InvalidOperationException(
                $"Inbox handler invocation failed for message type {messageType.Name}");

        await task;
    }
}
