
using AuthService.Application.IntegrationEvents;

namespace AuthService.Infrastructure.Messaging.Routing;

public static class RoutingKeyMap
{
    private static readonly Dictionary<Type, string> _map = new()
    {
        { typeof(UserCreatedIntegrationEvent), "user.created" },
        { typeof(UserActivatedIntegrationEvent), "user.activated" },
        { typeof(UserDeactivatedIntegrationEvent), "user.deactivated" },
        { typeof(UserDeletedIntegrationEvent), "user.deleted" },
        
    };

    public static string Get(Type type)
    {
        if (!_map.TryGetValue(type, out var key))
            throw new Exception($"Routing key not found for {type.Name}");

        return key;
    }
}
