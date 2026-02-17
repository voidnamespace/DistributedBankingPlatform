using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using MediatR;
namespace AccountService.Application.Queries.GetByIdAccount;

public class GetByIdAccountHandler 
    : IRequestHandler<GetByIdAccountQuery, ReadAccountDTO>
{

    private readonly IAccountRepository _accountRepository;


    public GetByIdAccountHandler (IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }


    public async Task<ReadAccountDTO> Handle (GetByIdAccountQuery query, CancellationToken ct)
    {
        var acc = await _accountRepository.GetByIdAsync(query.UserId, ct);

        if (acc == null)
            throw new KeyNotFoundException("no account found");

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
