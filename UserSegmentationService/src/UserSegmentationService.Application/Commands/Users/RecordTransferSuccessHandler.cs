using MediatR;
using Microsoft.Extensions.Logging;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Domain.Entities;
using UserSegmentationService.Domain.Enums;

namespace UserSegmentationService.Application.Commands.Users;

public class RecordTransferSuccessHandler
    : IRequestHandler<RecordTransferSuccessCommand>
{
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUserMetricRepository _userMetricRepository;
    private readonly ILogger<RecordTransferSuccessHandler> _logger;

    public RecordTransferSuccessHandler(
        IUserAccountRepository userAccountRepository,
        IUserMetricRepository userMetricRepository,
        ILogger<RecordTransferSuccessHandler> logger)
    {
        _userAccountRepository = userAccountRepository;
        _userMetricRepository = userMetricRepository;
        _logger = logger;

    }

    public async Task Handle(
        RecordTransferSuccessCommand command,
        CancellationToken cancellationToken)
    {
        using var logScope = _logger.BeginScope(
            new Dictionary<string, object?>
            {
                ["TransactionId"] = command.TransactionId,
                ["FromAccountNumber"] = MaskAccountNumber(command.FromAccountNumber),
                ["ToAccountNumber"] = MaskAccountNumber(command.ToAccountNumber)
            });

        var senderAccount = await _userAccountRepository.GetByAccountNumberAsync(
            command.FromAccountNumber,
            cancellationToken);

        if (senderAccount is null)
        {
            _logger.LogWarning(
                "Transfer success skipped because sender account projection was not found");
            return;
        }

        var metric = await _userMetricRepository.GetByUserIdAsync(
            senderAccount.UserId,
            cancellationToken);

        var metricCreated = false;

        if (metric is null)
        {
            metric = new UserMetric(senderAccount.UserId);
            _userMetricRepository.Add(metric);
            metricCreated = true;
        }

        var amountInCopper = ConvertToCopper(command.Amount, command.Currency);

        metric.RecordSpend(amountInCopper, command.RecordedAt);

        _logger.LogInformation(
            "Transfer success recorded for user metric. UserId={UserId}, Currency={Currency}, MetricCreated={MetricCreated}, RecordedAt={RecordedAt}",
            senderAccount.UserId,
            command.Currency,
            metricCreated,
            command.RecordedAt);
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return "<empty>";

        const int visibleDigits = 4;

        return accountNumber.Length <= visibleDigits
            ? "****"
            : string.Concat("****", accountNumber.AsSpan(accountNumber.Length - visibleDigits));
    }

    private static decimal ConvertToCopper(decimal amount, int currency)
    {
        if (!Enum.IsDefined(typeof(Currency), currency))
            throw new InvalidOperationException(
                $"Unsupported currency value '{currency}'.");

        var parsedCurrency = (Currency)currency;

        return parsedCurrency switch
        {
            Currency.Copper => amount,
            Currency.Silver => amount * 100m,
            Currency.Gold => amount * 10_000m,
            _ => throw new InvalidOperationException(
                $"Unsupported currency value '{currency}'.")
        };
    }

}
