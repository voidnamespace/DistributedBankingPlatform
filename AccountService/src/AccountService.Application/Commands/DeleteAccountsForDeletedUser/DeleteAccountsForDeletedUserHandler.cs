using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AccountService.Application.Commands.DeleteAccountEventChain;

public class DeleteAccountsForDeletedUserHandler : IRequestHandler<DeleteAccountsForDeletedUserCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteAccountsForDeletedUserHandler> _logger;

    public DeleteAccountsForDeletedUserHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteAccountsForDeletedUserHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteAccountsForDeletedUserCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "DeleteAccountsForDeletedUserCommand started for user {UserId}",
            command.UserId);

        var accounts = await _accountRepository
            .GetByUserIdAsync(command.UserId, ct);

        if (accounts == null || !accounts.Any())
        {
            _logger.LogInformation(
                "DeleteAccountsForDeletedUserCommand: no accounts found for user {UserId}",
                command.UserId);

            return;
        }

        _logger.LogInformation(
            "DeleteAccountsForDeletedUserCommand: {Count} accounts loaded for deletion for user {UserId}",
            accounts.Count(),
            command.UserId);

        foreach (var account in accounts)
        {
            await _accountRepository.DeleteAsync(account, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "DeleteAccountsForDeletedUserCommand completed: deleted {Count} accounts for user {UserId}",
            accounts.Count(),
            command.UserId);
    }
}
