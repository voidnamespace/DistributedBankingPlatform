using AccountService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
namespace AccountService.Application.Commands.DeactivateAccountEventChain;

public class DeactivateAccountEventChainHandler
    : IRequestHandler<DeactivateAccountEventChainCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeactivateAccountEventChainHandler> _logger;

    public DeactivateAccountEventChainHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeactivateAccountEventChainHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeactivateAccountEventChainCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "DeactivateAccountEventChainCommand received for user {UserId}",
            command.UserId);

        var accounts = await _accountRepository.GetByUserIdAsync(command.UserId, ct);

        foreach (var account in accounts)
        {
            account.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Deactivated {Count} accounts for user {UserId}",
            accounts.Count,
            command.UserId);
    }
}