using BankCardService.Application.DTOs;
using BankCardService.Application.Interfaces;
using MediatR;

namespace BankCardService.Application.Queries.GetBankCardById;

public class GetBankCardByIdHandler
    : IRequestHandler<GetBankCardByIdQuery, BankCardDTO>
{
    private readonly IBankCardRepository _bankCardRepository;

    public GetBankCardByIdHandler(IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }

    public async Task<BankCardDTO> Handle(
        GetBankCardByIdQuery request,
        CancellationToken cancellationToken)
    {
        var card = await _bankCardRepository.GetByIdAsync(request.CardId);

        if (card == null)
            throw new KeyNotFoundException("Bank card not found");

        return new BankCardDTO
        {
            Id = card.Id,
            CardNumber = card.CardNumber.Value,
            CardHolder = card.CardHolder,
            ExpirationDate = card.ExpirationDate,
            IsActive = card.IsActive
        };
    }
}