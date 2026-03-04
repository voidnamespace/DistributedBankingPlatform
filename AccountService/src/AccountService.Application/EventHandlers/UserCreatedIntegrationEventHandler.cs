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
        var command = new CreateAccountCommand(
            notification.UserId,
            Currency.Copper
        );

        await _mediator.Send(command, ct);
    }
}