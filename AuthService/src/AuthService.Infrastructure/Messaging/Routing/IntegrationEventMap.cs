using AuthService.Application.IntegrationEvents;

namespace AuthService.Infrastructure.Messaging.Routing;

public static class IntegrationEventMap
{
    private static readonly Dictionary<Type, string> TypeToName = new()
    {
        { typeof(UserCreatedIntegrationEvent), "user.created" },
        { typeof(UserActivatedIntegrationEvent), "user.activated" },
        { typeof(UserDeactivatedIntegrationEvent), "user.deactivated" },
        { typeof(UserDeletedIntegrationEvent), "user.deleted" },
    };

    private static readonly Dictionary<string, Type> NameToType =
        TypeToName.ToDictionary(x => x.Value, x => x.Key);

    public static string GetName(Type type)
    {
        if (!TypeToName.TryGetValue(type, out var name))
            throw new Exception($"Event name not found for {type.Name}");

        return name;
    }

    public static Type GetType(string name)
    {
        if (!NameToType.TryGetValue(name, out var type))
            throw new Exception($"Event type not found for {name}");

        return type;
    }
}
