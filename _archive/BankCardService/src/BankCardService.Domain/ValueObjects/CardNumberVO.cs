namespace BankCardService.Domain.ValueObjects;
public class CardNumberVO
{
    private CardNumberVO() { } 

    private string _bankCard = string.Empty;
    public string Value => _bankCard;

    public CardNumberVO(string bankCard)
    {
        if (string.IsNullOrWhiteSpace(bankCard))
            throw new ArgumentNullException(nameof(bankCard), "Card number cannot be empty");

        if (bankCard.Length != 16 || !bankCard.All(char.IsDigit))
            throw new ArgumentException("Card number must be 16 digits");

        if (!IsValidLuhn(bankCard))
            throw new ArgumentException("Invalid card number");

        _bankCard = bankCard;
    }

    private bool IsValidLuhn(string number)
    {
        int sum = 0;
        bool alternate = false;

        for (int i = number.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(number[i].ToString());
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }
}
