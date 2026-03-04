using AccountService.Infrastructure.Persistence.Inbox;
using AccountService.Infrastructure.Data;
using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.IntegrationEvents;
using AccountService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace AccountService.Infrastructure.Persistence.Inbox;

public class InboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public InboxProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<AccountDbContext>();

            var mediator = scope.ServiceProvider
                .GetRequiredService<IMediator>();

            var messages = await db.InboxMessages
                .Where(x => x.ProcessedAt == null)
                .OrderBy(x => x.ReceivedAt)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    if (message.Type == nameof(UserCreatedIntegrationEvent))
                    {
                        var integrationEvent =
                            JsonSerializer.Deserialize<UserCreatedIntegrationEvent>(message.Payload);

                        if (integrationEvent == null)
                            continue;

                        if (!Enum.TryParse<Currency>(
                                integrationEvent.Currency,
                                true,
                                out var currency))
                        {
                            throw new Exception($"Invalid currency: {integrationEvent.Currency}");
                        }

                        await mediator.Send(
                            new CreateAccountCommand(
                                integrationEvent.UserId,
                                currency),
                            stoppingToken);
                    }

                    message.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.Error = ex.Message;
                }
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }
}