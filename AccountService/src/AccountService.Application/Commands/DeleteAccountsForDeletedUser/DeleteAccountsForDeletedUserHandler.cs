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
            "DeleteAccountEventChainCommand received for user {UserId}",
            command.UserId);

        var accounts = await _accountRepository.GetByUserIdAsync(command.UserId, ct);

        if (accounts == null || !accounts.Any())
        {
            _logger.LogInformation(
                "No accounts found for user {UserId}",
                command.UserId);

            return;
        }

        foreach (var account in accounts)
        {
            await _accountRepository.DeleteAsync(account, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deleted {Count} accounts for user {UserId}",
            accounts.Count(),
            command.UserId);
    }
}