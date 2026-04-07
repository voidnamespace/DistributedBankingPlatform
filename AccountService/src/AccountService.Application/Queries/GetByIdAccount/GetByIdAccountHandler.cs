using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Queries.GetByIdAccount;

public class GetByIdAccountHandler
    : IRequestHandler<GetByIdAccountQuery, ReadAccountDTO>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetByIdAccountHandler> _logger;

    public GetByIdAccountHandler(
        IAccountRepository accountRepository,
        ILogger<GetByIdAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<ReadAccountDTO> Handle(
        GetByIdAccountQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetByIdAccountQuery received for account {AccountId}, user {UserId}",
            query.AccountId,
            query.TokenUserId);


        var acc = await _accountRepository
            .GetByIdAsync(query.AccountId, ct);


        if (acc == null)
        {
            _logger.LogWarning(
                "GetByIdAccountQuery failed: account {AccountId} not found",
                query.AccountId);

            throw new KeyNotFoundException("Account not found");
        }


        if (acc.UserId != query.TokenUserId)
        {
            _logger.LogWarning(
                "GetByIdAccountQuery failed: ownership mismatch for account {AccountId}, user {UserId}",
                query.AccountId,
                query.TokenUserId);

            throw new UnauthorizedAccessException();
        }


        var dto = new ReadAccountDTO
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


        _logger.LogInformation(
            "GetByIdAccountQuery completed successfully for account {AccountId}, user {UserId}",
            query.AccountId,
            query.TokenUserId);

        return dto;
    }
}
