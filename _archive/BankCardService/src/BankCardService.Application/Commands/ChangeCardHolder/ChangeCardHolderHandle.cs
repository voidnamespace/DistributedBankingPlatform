using BankCardService.Application.Interfaces;
using MediatR;

namespace BankCardService.Application.Commands.ChangeCardHolder;

public class ChangeCardHolderHandle : IRequestHandler<ChangeCardHolderCommand>
{
    private readonly IBankCardRepository _bankCardRepository;

    public ChangeCardHolderHandle(IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }


    public async Task Handle (ChangeCardHolderCommand request, CancellationToken cancellationToken)
    {
        var card = await _bankCardRepository.GetByIdAsync(request.cardId);
        if (card == null)
            throw new KeyNotFoundException("Bank card not found");
        card.ChangeCardHolder(request.newCardHolder, DateTime.UtcNow);
        await _bankCardRepository.SaveAsync();

    }




}
