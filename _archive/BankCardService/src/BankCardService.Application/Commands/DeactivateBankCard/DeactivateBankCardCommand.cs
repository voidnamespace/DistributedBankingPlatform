using MediatR;
namespace BankCardService.Application.Commands.DeactivateBankCard;

public record DeactivateBankCardCommand(Guid cardId) : IRequest;

