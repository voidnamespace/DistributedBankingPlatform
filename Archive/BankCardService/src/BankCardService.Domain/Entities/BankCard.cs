using BankCardService.Domain.Common;
using BankCardService.Domain.ValueObjects;
using BankCardService.Domain.Events;
namespace BankCardService.Domain.Entities;
public class BankCard : AggregateRoot
{
    public Guid Id { get; private set; }
    public CardNumberVO CardNumber { get; private set; } = null!;
    public string CardHolder { get; private set; } = null!;
    public DateTime ExpirationDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }


    private BankCard() { }

    public BankCard (string cardNumber, string cardHolder, DateTime nowUtc)
    {
        Id = Guid.NewGuid();
        CardNumber = new CardNumberVO(cardNumber);
        ChangeCardHolder(cardHolder, nowUtc); 
        ExpirationDate = nowUtc.AddYears(7);
        CreatedAt = nowUtc;
        IsActive = true;
    }

    public void ChangeCardNumber(CardNumberVO newCardNumber, DateTime nowUtc)
    {
        CardNumber = newCardNumber;
        UpdatedAt = nowUtc;
    }

    public void ChangeCardHolder(string newCardHolder, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(newCardHolder))
            throw new ArgumentException("CardHolder cannot be empty");
        CardHolder = newCardHolder;
        UpdatedAt = nowUtc;
    }

    public void Activate(DateTime nowUtc)
    {
        if (IsActive)
            throw new InvalidOperationException("Card already active");

        IsActive = true;
        UpdatedAt = nowUtc;

        AddDomainEvent(new BankCardActivatedEvent(Id, nowUtc));
    }

    public void Deactivate(DateTime nowUtc)
    {
        if (!IsActive)
            throw new InvalidOperationException("Card already inactive");

        IsActive = false;
        UpdatedAt = nowUtc;

        AddDomainEvent(new BankCardDeactivatedEvent(Id, nowUtc));
    }


}
