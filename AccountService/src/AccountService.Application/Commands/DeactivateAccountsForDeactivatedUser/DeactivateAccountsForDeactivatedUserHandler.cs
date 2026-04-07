using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.DeactivateAccountEventChain;

public class DeactivateAccountsForDeactivatedUserHandler
    : IRequestHandler<DeactivateAccountsForDeactivatedUserCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateAccountsForDeactivatedUserHandler> _logger;

    public DeactivateAccountsForDeactivatedUserHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateAccountsForDeactivatedUserHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeactivateAccountsForDeactivatedUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "DeactivateAccountsForDeactivatedUserCommand started for {UserId} after user deactivation event",
            command.UserId);

        var accounts = await _accountRepository.GetByUserIdAsync(
            command.UserId,
            ct);

        _logger.LogInformation(
            "Account entities loaded from database for {UserId}, {Count} accounts",
            command.UserId,
            accounts.Count);

        if (accounts.Count == 0)
        {
            _logger.LogInformation(
                "No accounts found for {UserId} to deactivate",
                command.UserId);

            return;
        }

        foreach (var account in accounts)
        {
            account.Deactivate();
        }

        _logger.LogInformation(
            "Account entities deactivated for {UserId}, {Count} accounts",
            command.UserId,
            accounts.Count);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DeactivateAccountsForDeactivatedUserCommand completed for {UserId}, {Count} accounts",
            command.UserId,
            accounts.Count);
    }
}
