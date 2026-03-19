using AccountService.Application.IntegrationEvents.Transactions;

namespace AccountService.Infrastructure.Messaging.Routing;

public static class RoutingKeyMap
{
    private static readonly Dictionary<Type, string> _map = new()
    {
        { typeof(TransferCreatedIntegrationEvent), "transaction.created" }
    };

    public static string Get(Type type)
    {
        if (!_map.TryGetValue(type, out var key))
            throw new Exception($"Routing key not found for {type.Name}");

        return key;
    }
}
