using AccountService.Application.DTOs;
using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;
using AccountService.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Queries.GetByAccountNumberAccount;

public class GetByAccountNumberAccountHandler
    : IRequestHandler<GetByAccountNumberAccountQuery, ReadAccountDTO>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetByAccountNumberAccountHandler> _logger;

    public GetByAccountNumberAccountHandler(
        IAccountRepository accountRepository,
        ILogger<GetByAccountNumberAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<ReadAccountDTO> Handle(
        GetByAccountNumberAccountQuery query,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "GetByAccountNumberAccountQuery received for account {AccountNumber}, user {UserId}",
            query.AccountNumber,
            query.UserId);


        var accountNumberVO = new AccountNumberVO(query.AccountNumber);

        var acc = await _accountRepository
            .GetByAccountNumberAsync(accountNumberVO, ct);


        if (acc == null)
        {
            _logger.LogWarning(
                "GetByAccountNumberAccountQuery failed: account {AccountNumber} not found",
                query.AccountNumber);

            throw new DomainException("Account not found");
        }


        if (acc.UserId != query.UserId)
        {
            _logger.LogWarning(
                "GetByAccountNumberAccountQuery failed: ownership mismatch for account {AccountNumber}, user {UserId}",
                query.AccountNumber,
                query.UserId);

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
            "GetByAccountNumberAccountQuery completed successfully for account {AccountNumber}",
            query.AccountNumber);

        return dto;
    }
}
