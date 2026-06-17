using MassTransit;
using TransactionKafkaBridge.Application.IntegrationEvents.Transfer;
using TransactionKafkaBridge.Infrastructure.Kafka;

namespace TransactionKafkaBridge.Infrastructure.MasstransitConsumers.Transfer;

public sealed class TransferSuccessConsumer : IConsumer<TransferSuccessIntegrationEvent>
{
    private readonly TransferKafkaProducer _producer;

    public TransferSuccessConsumer(TransferKafkaProducer producer)
    {
        _producer = producer;
    }

    public Task Consume(ConsumeContext<TransferSuccessIntegrationEvent> context)
    {
        return _producer.PublishTransferSuccessAsync(
            context.Message,
            context.CancellationToken);
    }
}
