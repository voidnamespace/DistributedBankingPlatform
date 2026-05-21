using UserSegmentationService.Application.IntegrationEvents.Accounts;

namespace UserSegmentationService.Infrastructure.Outbox;

internal static class IntegrationEventTypeMap
{

    private static readonly Dictionary<Type, string> TypeToName = new()
    {
        {typeof(UserAccountsBackfillRequestedIntegrationEvent), "segment.backfill" }
    };

    private static readonly Dictionary<string, Type> NameToType =
        TypeToName.ToDictionary(x => x.Value, x  => x.Key);



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
