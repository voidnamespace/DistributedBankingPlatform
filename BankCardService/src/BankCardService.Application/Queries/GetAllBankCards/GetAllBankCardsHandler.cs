using BankCardService.Application.DTOs;
using BankCardService.Application.Interfaces;
using MediatR;


namespace BankCardService.Application.Queries.GetAllBankCards;

public class GetAllBankCardsHandler : IRequestHandler<GetAllBankCardsQuery, IEnumerable<BankCardDTO>>

{
    private readonly IBankCardRepository _bankCardRepository;

    public GetAllBankCardsHandler (IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }


    public async Task<IEnumerable<BankCardDTO>> Handle (GetAllBankCardsQuery request, CancellationToken cancellationToken)
    {
        var cards = await _bankCardRepository.GetAllAsync();

        return cards.Select(card => new BankCardDTO
        {
            Id = card.Id,
            CardNumber = card.CardNumber.Value,
            CardHolder = card.CardHolder,
            ExpirationDate = card.ExpirationDate,
            IsActive = card.IsActive
        });
    }
}
