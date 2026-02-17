namespace BankCardService.Application.DTOs;

public class CreateBankCardDTO
{
    public string CardNumber { get; set; } = null!;
    public string CardHolder { get; set; } = null!;
}
