using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using TransactionKafkaBridge.Application.IntegrationEvents.Transfer;

namespace TransactionKafkaBridge.Infrastructure.Kafka;

public sealed class TransferKafkaProducer : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;

    public TransferKafkaProducer(IOptions<KafkaOptions> options)
    {
        _options = options.Value;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishTransferSuccessAsync(
        TransferSuccessIntegrationEvent integrationEvent,
        CancellationToken cancellationToken)
    {
        var message = new Message<string, string>
        {
            Key = integrationEvent.TransactionId.ToString(),
            Value = JsonSerializer.Serialize(integrationEvent),
            Headers = new Headers
            {
                { "event-type", "transfer.success"u8.ToArray() }
            }
        };

        await _producer.ProduceAsync(
            _options.TransferSuccessTopic,
            message,
            cancellationToken);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
