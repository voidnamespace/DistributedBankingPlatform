using AccountService.Domain.Enums;

namespace AccountService.Application.DTOs;

public class CreateAccountRequest
{
    public Guid UserId { get; set; }
    public Currency Currency { get; set; }
}

public class CreateAccountResponse
{
    public Guid AccountId { get; init; }
    public string AccountNumber { get; init; } = default!;
}
