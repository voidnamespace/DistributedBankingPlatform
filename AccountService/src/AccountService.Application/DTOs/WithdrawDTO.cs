using AccountService.Domain.Enums;
namespace AccountService.Application.DTOs;

public class WithdrawRequest
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; } 
}
