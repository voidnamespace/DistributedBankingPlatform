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
            "DeleteAccountCommand received for account {AccountId} by user {UserId}",
            command.AccountId,
            command.UserId);

        var account = await _accountRepository.GetByIdAsync(command.AccountId, ct);

        if (account == null)
            throw new KeyNotFoundException($"Account with ID {command.AccountId} not found");

        if (account.UserId != command.UserId)
            throw new InvalidOperationException("U can delete only your account");

        await _accountRepository.DeleteAsync(command.AccountId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Account {AccountId} deleted by user {UserId}",
            command.AccountId,
            command.UserId);
    }
}