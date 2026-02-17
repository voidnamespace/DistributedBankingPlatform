namespace BankCardService.Application.DTOs;

public class BankCardDTO
{
    public Guid Id { get; set; }
    public required string CardNumber { get; set; } 
    public required string CardHolder { get; set; } 
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}
