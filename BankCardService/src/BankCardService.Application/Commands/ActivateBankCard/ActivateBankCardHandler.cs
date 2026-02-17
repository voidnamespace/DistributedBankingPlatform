using MediatR;
using BankCardService.Application.Interfaces;
namespace BankCardService.Application.Commands.ActivateBankCard;

public class ActivateBankCardHandler : IRequestHandler<ActivateBankCardCommand>
{
    private readonly IBankCardRepository _bankCardRepository;
    public ActivateBankCardHandler (IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }


    public async Task Handle(ActivateBankCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _bankCardRepository.GetByIdAsync(request.cardId);
        if (card == null)
            throw new KeyNotFoundException("Bank card not found");
        card.Activate(DateTime.UtcNow);
        await _bankCardRepository.SaveAsync();

    }
}
