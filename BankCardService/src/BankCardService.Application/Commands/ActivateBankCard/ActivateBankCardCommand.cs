using MediatR;
namespace BankCardService.Application.Commands.ActivateBankCard;

public record ActivateBankCardCommand(Guid cardId) : IRequest;

