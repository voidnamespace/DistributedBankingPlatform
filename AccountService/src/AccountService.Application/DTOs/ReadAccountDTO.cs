using AccountService.Domain.Enums;

namespace AccountService.Application.DTOs;

public class ReadAccountDTO
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string AccountNumber { get; init; } = default!;
    public decimal BalanceAmount { get; set; }
    public Currency BalanceCurrency { get; set; } = default!;
    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public bool IsActive { get; init; }
}
