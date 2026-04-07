using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.DeactivateAccount;

public class DeactivateAccountHandler
    : IRequestHandler<DeactivateAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateAccountHandler> _logger;

    public DeactivateAccountHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        DeactivateAccountCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "DeactivateAccountCommand started for {AccountId}",
            request.AccountId);

        var account = await _accountRepository.GetByIdAsync(
            request.AccountId,
            ct);

        if (account == null)
            throw new KeyNotFoundException("Account not found");

        _logger.LogInformation(
            "Account entity loaded from database {AccountId}",
            request.AccountId);

        account.Deactivate();

        _logger.LogInformation(
            "Account entity deactivated {AccountId}",
            request.AccountId);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DeactivateAccountCommand completed for {AccountId} ",
            request.AccountId);
    }
}
