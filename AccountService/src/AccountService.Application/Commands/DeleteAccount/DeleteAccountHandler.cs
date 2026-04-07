using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.DeleteAccount;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAccountHandler> _logger;

    public DeleteAccountHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAccountHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "DeleteAccountCommand started for account {AccountId} by user {UserId}",
            command.AccountId,
            command.UserId);

        var account = await _accountRepository.GetByIdAsync(command.AccountId, ct);

        if (account == null)
        {
            _logger.LogWarning(
                "DeleteAccountCommand failed: account {AccountId} not found",
                command.AccountId);

            throw new KeyNotFoundException(
                $"Account with ID {command.AccountId} not found");
        }

        if (account.UserId != command.UserId)
        {
            _logger.LogWarning(
                "DeleteAccountCommand forbidden: user {UserId} tried deleting account {AccountId}",
                command.UserId,
                command.AccountId);

            throw new InvalidOperationException(
                "U can delete only your account");
        }

        account.Delete();

        await _accountRepository.DeleteAsync(account, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DeleteAccountCommand completed for {UserId}, {AccountId}",
            command.UserId,
            command.AccountId);
    }
}
