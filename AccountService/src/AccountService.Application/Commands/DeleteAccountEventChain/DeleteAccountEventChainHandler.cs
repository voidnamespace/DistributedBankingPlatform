using AccountService.Application.Interfaces;
using MediatR;
namespace AccountService.Application.Commands.DeleteAccountEventChain;

public class DeleteAccountEventChainHandler : IRequestHandler<DeleteAccountEventChainCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountEventChainHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAccountEventChainCommand command, CancellationToken ct)
    {
        var accounts = await _accountRepository.GetByUserIdAsync(command.UserId, ct);

        if (accounts == null || !accounts.Any())
            return;

        foreach (var account in accounts)
        {
            await _accountRepository.DeleteAsync(account.Id, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }
}
