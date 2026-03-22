using AccountService.Application.IntegrationEvents.Users;

namespace AccountService.Infrastructure.Messaging.Routing;

internal static class AuthConsumerRoutingMap
{
    private static readonly Dictionary<string, Type> _map = new()
    {
        { "user.created", typeof(UserCreatedIntegrationEvent) },
        { "user.deleted", typeof(UserDeletedIntegrationEvent) },
        { "user.activated", typeof(UserActivatedIntegrationEvent) },
        { "user.deactivated", typeof(UserDeactivatedIntegrationEvent) }
    };

    public static bool TryGet(string routingKey, out Type type)
    {
        return _map.TryGetValue(routingKey, out type!);
    }
}