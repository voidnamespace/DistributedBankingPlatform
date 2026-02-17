using AccountService.Domain.Enums;

namespace AccountService.Application.DTOs;

public class DepositRequest
{
    public decimal Amount { get; init; }
    public Currency Currency { get; init; } 
}
