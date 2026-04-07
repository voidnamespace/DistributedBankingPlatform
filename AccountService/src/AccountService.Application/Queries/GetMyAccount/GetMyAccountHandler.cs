using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Queries.GetMyAccount;

public class GetMyAccountHandler
    : IRequestHandler<GetMyAccountQuery, IReadOnlyList<ReadAccountDTO>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetMyAccountHandler> _logger;

    public GetMyAccountHandler(
        IAccountRepository accountRepository,
        ILogger<GetMyAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReadAccountDTO>> Handle(
        GetMyAccountQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetMyAccountQuery received for user {UserId}",
            query.UserId);


        var accounts = await _accountRepository
            .GetByUserIdAsync(query.UserId, ct);


        if (accounts.Count == 0)
        {
            _logger.LogInformation(
                "GetMyAccountQuery completed: no accounts found for user {UserId}",
                query.UserId);

            return [];
        }


        var dtoList = accounts.Select(acc => new ReadAccountDTO
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


        _logger.LogInformation(
            "GetMyAccountQuery completed successfully for user {UserId}, accounts found: {Count}",
            query.UserId,
            dtoList.Count);

        return dtoList;
    }
}
