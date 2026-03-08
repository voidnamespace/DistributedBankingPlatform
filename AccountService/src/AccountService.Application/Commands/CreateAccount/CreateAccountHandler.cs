using AccountService.Application.Interfaces;
using AccountService.Domain.Entity;
using AccountService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.CreateAccount;

public class CreateAccountHandler
    : IRequestHandler<CreateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateAccountHandler> _logger;

    public CreateAccountHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        CreateAccountCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "CreateAccountCommand received for user {UserId} with currency {Currency}",
            command.UserId,
            command.Currency);

        AccountNumberVO accountNumberVO;

        do
        {
            accountNumberVO = AccountNumberVO.Generate();
        }
        while (await _accountRepository
            .ExistsByAccountNumberAsync(accountNumberVO, ct));

        var acc = new Account(
            command.UserId,
            accountNumberVO,
            command.Currency);

        await _accountRepository.AddAsync(acc, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Account created for user {UserId} with account number {AccountNumber}",
            command.UserId,
            accountNumberVO.Value);
    }
}