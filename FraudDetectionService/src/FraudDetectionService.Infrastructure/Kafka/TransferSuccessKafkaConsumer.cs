using System.Text.Json;
using Confluent.Kafka;
using FraudDetectionService.Application.IntegrationEvents.Transfer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FraudDetectionService.Infrastructure.Kafka;

public sealed class TransferSuccessKafkaConsumer : BackgroundService
{
    private readonly ILogger<TransferSuccessKafkaConsumer> _logger;
    private readonly KafkaOptions _options;

    public TransferSuccessKafkaConsumer(
        ILogger<TransferSuccessKafkaConsumer> logger,
        IOptions<KafkaOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        consumer.Subscribe(_options.TransferSuccessTopic);

        _logger.LogInformation(
            "Kafka consumer subscribed to topic {Topic}",
            _options.TransferSuccessTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = consumer.Consume(stoppingToken);
                var integrationEvent = JsonSerializer.Deserialize<TransferSuccessIntegrationEvent>(
                    result.Message.Value);

                if (integrationEvent is null)
                {
                    _logger.LogWarning(
                        "Kafka message could not be deserialized. Topic={Topic} Offset={Offset}",
                        result.Topic,
                        result.Offset);
                    continue;
                }

                _logger.LogInformation(
                    "Transfer success event consumed from Kafka. TransactionId={TransactionId} Amount={Amount} Currency={Currency}",
                    integrationEvent.TransactionId,
                    integrationEvent.Amount,
                    integrationEvent.Currency);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            consumer.Close();
        }
    }
}
