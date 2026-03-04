using AccountService.Application.Commands.CreateAccount;
using AccountService.Application.IntegrationEvents;
using AccountService.Domain.Enums;
using MediatR;

public class UserCreatedIntegrationEventHandler
    : INotificationHandler<UserCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(
        UserCreatedIntegrationEvent notification,
        CancellationToken ct)
    {
        if (!Enum.TryParse<Currency>(
            notification.Currency,
            true,
            out var currency))
        {
            throw new Exception($"Invalid currency: {notification.Currency}");
        }

        await _mediator.Send(
            new CreateAccountCommand(
                notification.UserId,
                currency),
            ct);
    }
}