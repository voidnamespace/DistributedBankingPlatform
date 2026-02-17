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
        var user = await _accountRepository.GetByIdAsync(command.accId, ct);
        if (user == null)
            throw new KeyNotFoundException($"Account with ID {command.accId} not found");
        
        await _accountRepository.DeleteAsync(command.accId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

    }




}
