using BankCardService.Application.Interfaces;
using MediatR;
namespace BankCardService.Application.Commands.DeactivateBankCard;

public class DeactivateBankCardHandler : IRequestHandler<DeactivateBankCardCommand>
{
    private readonly IBankCardRepository _bankCardRepository;
    public DeactivateBankCardHandler (IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }

    public async Task Handle(DeactivateBankCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _bankCardRepository.GetByIdAsync(request.cardId);
        if (card == null)
            throw new KeyNotFoundException("Bank Card not found");
        card.Deactivate(DateTime.UtcNow);
        await _bankCardRepository.SaveAsync();
    }
}
