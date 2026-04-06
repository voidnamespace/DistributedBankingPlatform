using MediatR;
namespace BankCardService.Application.Commands.ChangeCardHolder;

public record ChangeCardHolderCommand(Guid cardId, string newCardHolder) : IRequest;

