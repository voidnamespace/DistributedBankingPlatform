using AccountService.Domain.Enums;

namespace AccountService.Application.DTOs;

public record CreateAccountRequest(
    Currency Currency
);

public record CreateAccountResponse
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = default!;
}
