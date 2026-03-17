using AccountService.Application.Interfaces;
using AccountService.Domain.ValueObjects;
using MediatR;
namespace AccountService.Application.Commands.TransferMoney;

public class TransferMoneyHandler
{

    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferMoneyHandler(IAccountRepository accountRepository, 
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TransferMoneyCommand command, CancellationToken ct)
    {
            if (command.FromAccountId == command.ToAccountId)
                throw new ArgumentException("Not able to tranfer to same account");
            if (command.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0");

            var fromAccVO = new AccountNumberVO(command.FromAccountId.ToString());
            var toAccVo = new AccountNumberVO(command.ToAccountId.ToString());

            var fromAccount = await _accountRepository.GetByAccountNumberAsync(fromAccVO, ct);
            var toAccount = await _accountRepository.GetByAccountNumberAsync(toAccVo, ct);

            if (fromAccount == null || toAccount == null)
                throw new ArgumentException("Wrong account");

            if (fromAccount.Balance.Currency != toAccount.Balance.Currency)
                throw new ArgumentException("Invalid currency");

            var moneyVO = new MoneyVO(command.Amount, command.Currency);

            fromAccount.TransferTo(toAccount, moneyVO);


            await _unitOfWork.SaveChangesAsync(ct);
       
    }

}
