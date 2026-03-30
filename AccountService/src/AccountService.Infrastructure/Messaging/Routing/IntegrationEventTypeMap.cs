using AccountService.Application.IntegrationEvents.Accounts;
using AccountService.Application.IntegrationEvents.Transactions;
using AccountService.Application.IntegrationEvents.Users;

namespace AccountService.Infrastructure.Messaging.Routing;

internal static class IntegrationEventTypeMap
{
    private static readonly Dictionary<Type, string> TypeToName = new()
    {
        { typeof(UserCreatedIntegrationEvent), "user.created" },
        { typeof(UserDeletedIntegrationEvent), "user.deleted" },
        { typeof(UserActivatedIntegrationEvent), "user.activated" },
        { typeof(UserDeactivatedIntegrationEvent), "user.deactivated" },

        { typeof(AccountCreatedIntegrationEvent), "account.created" },
        { typeof(AccountActivatedIntegrationEvent), "account.activated" },
        { typeof(AccountDeactivatedIntegrationEvent), "account.deactivated" },
        { typeof(AccountDeletedIntegrationEvent), "account.deleted" },
        { typeof(BalanceChangedIntegrationEvent), "account.balance" },

        { typeof(TransferCreatedIntegrationEvent), "transaction.created" },
        { typeof(TransferSuccessIntegrationEvent), "transaction.success" },
        { typeof(TransferFailedIntegrationEvent), "transaction.failed" }
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
