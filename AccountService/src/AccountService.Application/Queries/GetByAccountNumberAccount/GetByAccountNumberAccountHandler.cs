using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;
using AccountService.Domain.Exceptions;
namespace AccountService.Application.Queries.GetByAccountNumberAccount;

public class GetByAccountNumberAccountHandler : IRequestHandler<GetByAccountNumberAccountQuery, ReadAccountDTO>
{
    private readonly IAccountRepository _accountRepository;


    public GetByAccountNumberAccountHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<ReadAccountDTO> Handle(GetByAccountNumberAccountQuery query, CancellationToken ct)
    {
        var accountNumberVO = new AccountNumberVO(query.AccountId);

        var acc = await _accountRepository
            .GetByAccountNumberAsync(accountNumberVO, ct);

        if (acc == null)
            throw new DomainException("Account not found");

        return new ReadAccountDTO
        {
            Id = acc.Id,
            UserId = acc.UserId,
            AccountNumber = acc.AccountNumber.Value,
            BalanceAmount = acc.Balance.Amount,
            BalanceCurrency = acc.Balance.Currency,
            CreatedAt = acc.CreatedAt,
            UpdatedAt = acc.UpdatedAt,
            IsActive = acc.IsActive
        };
    }
}
