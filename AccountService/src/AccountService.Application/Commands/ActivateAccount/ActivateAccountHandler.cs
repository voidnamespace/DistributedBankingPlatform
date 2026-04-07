using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.ActivateAccount;

public class ActivateAccountHandler
    : IRequestHandler<ActivateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivateAccountHandler> _logger;

    public ActivateAccountHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        ActivateAccountCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "ActivateAccountCommand started for {AccountId} by {UserId}",
             request.AccountId,
             request.UserId);

        var account = await _accountRepository.GetByIdAsync(
            request.AccountId,
            ct);

        if (account == null)
            throw new KeyNotFoundException("Account not found");

        if (account.UserId != request.UserId)
            throw new UnauthorizedAccessException();

        _logger.LogInformation(
            "Account entity loaded from database {AccountId}",
            request.AccountId);

        account.Activate();

        _logger.LogInformation(
            "Account entity activated {AccountId}",
            request.AccountId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "ActivateAccountCommand completed for {AccountId} ",
            request.AccountId);
    }
}
