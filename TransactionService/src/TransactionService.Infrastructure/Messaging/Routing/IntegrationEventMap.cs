using TransactionService.Application.IntegrationEvents.Deposit;
using TransactionService.Application.IntegrationEvents.Transfer;
using TransactionService.Application.IntegrationEvents.Withdrawal;

namespace TransactionService.Infrastructure.Messaging.Routing;

public static class IntegrationEventMap
{
    private static readonly Dictionary<Type, string> TypeToName = new()
    {
        { typeof(TransferCreatedIntegrationEvent), "transfer.created" },
        { typeof(TransferSuccessIntegrationEvent), "transfer.success" },
        { typeof(TransferFailedIntegrationEvent), "transfer.failed" },

        { typeof(WithdrawalCreatedIntegrationEvent), "withdrawal.created" },
        { typeof(WithdrawalSuccessIntegrationEvent), "withdrawal.success" },
        { typeof(WithdrawalFailedIntegrationEvent), "withdrawal.failed" },

        { typeof(DepositCreatedIntegrationEvent), "deposit.created" },
        { typeof(DepositSuccessIntegrationEvent), "deposit.success" },

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
