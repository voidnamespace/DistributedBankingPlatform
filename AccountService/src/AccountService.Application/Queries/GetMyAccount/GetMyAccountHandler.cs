using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using MediatR;

namespace AccountService.Application.Queries.GetMyAccount;

public class GetMyAccountHandler
    : IRequestHandler<GetMyAccountQuery, IReadOnlyList<ReadAccountDTO>>
{
    private readonly IAccountRepository _accountRepository;

    public GetMyAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IReadOnlyList<ReadAccountDTO>> Handle(
        GetMyAccountQuery query,
        CancellationToken ct)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(query.UserId, ct);

        return accounts.Select(acc => new ReadAccountDTO
        {
            Id = acc.Id,
            UserId = acc.UserId,
            AccountNumber = acc.AccountNumber.Value,

            BalanceAmount = acc.Balance.Amount,
            BalanceCurrency = acc.Balance.Currency,

            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
            IsActive = acc.IsActive
        }).ToList();
    }
}