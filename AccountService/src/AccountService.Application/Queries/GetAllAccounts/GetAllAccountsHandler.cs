using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Queries.GetAllAccounts;

public class GetAllAccountsHandler
    : IRequestHandler<GetAllAccountsQuery, IReadOnlyList<ReadAccountDTO>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAllAccountsHandler> _logger;

    public GetAllAccountsHandler(
        IAccountRepository accountRepository,
        ILogger<GetAllAccountsHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReadAccountDTO>> Handle(
        GetAllAccountsQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation("GetAllAccountsQuery received");

        var accounts = await _accountRepository.GetAllAsync(ct);

        var result = accounts.Select(account => new ReadAccountDTO
        {
            Id = account.Id,
            UserId = account.UserId,
            AccountNumber = account.AccountNumber.Value,
            BalanceAmount = account.Balance.Amount,
            BalanceCurrency = account.Balance.Currency,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            IsActive = account.IsActive
        }).ToList();

        _logger.LogInformation(
            "GetAllAccountsQuery completed successfully. Returned {Count} accounts",
            result.Count);

        return result;
    }
}
