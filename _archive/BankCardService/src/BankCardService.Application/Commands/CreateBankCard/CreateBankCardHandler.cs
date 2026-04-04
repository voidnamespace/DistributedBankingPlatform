using BankCardService.Application.DTOs;
using BankCardService.Application.Interfaces;
using BankCardService.Domain.Entities;
using MediatR;
namespace BankCardService.Application.Commands.CreateBankCard;

public class CreateBankCardHandler : IRequestHandler<CreateBankCardCommand, BankCardDTO>
{
    private readonly IBankCardRepository _bankCardRepository;

    public CreateBankCardHandler(IBankCardRepository bankCardRepository)
    {
        _bankCardRepository = bankCardRepository;
    }

    public async Task<BankCardDTO> Handle(CreateBankCardCommand request, CancellationToken cancellationToken)
    {
        var card = new BankCard(
            cardNumber: request.CardNumber,
            cardHolder: request.CardHolder,
            DateTime.UtcNow
        );

        await _bankCardRepository.AddAsync(card);
        await _bankCardRepository.SaveAsync();

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
