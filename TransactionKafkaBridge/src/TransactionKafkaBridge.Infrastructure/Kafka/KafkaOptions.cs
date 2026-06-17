namespace TransactionKafkaBridge.Infrastructure.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";

    public string TransferSuccessTopic { get; init; } = "banking.transfer.success";
}
