using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.IntegrationEvents.Users;
using AccountService.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.IntegrationEventHandlers.Users;

public class UserCreatedIntegrationEventHandler
    : INotificationHandler<UserCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserCreatedIntegrationEventHandler> _logger;

    public UserCreatedIntegrationEventHandler(
        IMediator mediator,
        ILogger<UserCreatedIntegrationEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(
        UserCreatedIntegrationEvent notification,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "UserCreatedIntegrationEvent received for user {UserId}",
            notification.UserId);

        var command = new CreateAccountCommand(
            notification.UserId,
            Currency.Copper
        );

        await _mediator.Send(command, ct);

        _logger.LogInformation(
            "CreateAccountCommand sent for user {UserId}",
            notification.UserId);
    }
}
