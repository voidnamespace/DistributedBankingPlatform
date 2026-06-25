namespace FraudDetectionService.Infrastructure.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public string BootstrapServers { get; init; } = "localhost:9092";

    public string GroupId { get; init; } = "fraud-detection-service";

    public string TransferSuccessTopic { get; init; } = "banking.transfers.success.v1";
}
