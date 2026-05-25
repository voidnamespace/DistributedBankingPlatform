using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserSegmentationService.Application.Commands.Segments.ActiveUsers;
using UserSegmentationService.Application.Commands.Segments.RiskUsers;
using UserSegmentationService.Application.Commands.Segments.VipUsers;

namespace UserSegmentationService.Infrastructure.BackgroundJobs;

internal class SegmentEvaluationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SegmentEvaluationBackgroundService> _logger;

    public SegmentEvaluationBackgroundService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<SegmentEvaluationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(
            GetConfigurationValue("Segments:EvaluationIntervalSeconds", 60));

        await EvaluateSegmentsAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await EvaluateSegmentsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to evaluate dynamic segments");
            }
        }
    }

    private async Task EvaluateSegmentsAsync(CancellationToken cancellationToken)
    {
        var activeWindow = TimeSpan.FromSeconds(
            GetConfigurationValue("Segments:ActiveUsersWindowSeconds", 2_592_000));

        var riskWindow = TimeSpan.FromSeconds(
            GetConfigurationValue("Segments:RiskUsersWindowSeconds", 7_776_000));

        await using var scope = _scopeFactory.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var now = DateTime.UtcNow;

        await mediator.Send(
            new EvaluateActiveUserSegmentCommand(now.Subtract(activeWindow)),
            cancellationToken);

        await mediator.Send(
            new EvaluateVipUserSegmentCommand(
                GetConfigurationValue("Segments:VipUsersMinimumSpend", 5_000)),
            cancellationToken);

        await mediator.Send(
            new EvaluateRiskUserSegmentCommand(now.Subtract(riskWindow)),
            cancellationToken);

        _logger.LogInformation("Dynamic segments evaluated");
    }

    private int GetConfigurationValue(string key, int defaultValue)
    {
        return int.TryParse(_configuration[key], out var value)
            ? value
            : defaultValue;
    }
}
