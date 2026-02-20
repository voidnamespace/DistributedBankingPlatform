using AccountService.Application.Interfaces;
using MediatR;
namespace AccountService.Application.Commands.DeleteAccount;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountHandler (IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository; 
        _unitOfWork = unitOfWork;
    }

    public async Task Handle (DeleteAccountCommand command, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(command.AccountId, ct);
        if (account == null)
            throw new KeyNotFoundException($"Account with ID {command.AccountId} not found");
        
        await _accountRepository.DeleteAsync(command.AccountId, ct);
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
