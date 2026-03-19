using AccountService.Application.IntegrationEvents.Transactions;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.ValueObjects;
using MediatR;
using System.ComponentModel;
namespace AccountService.Application.Commands.TransferMoney;

public class TransferMoneyHandler
{

    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    public TransferMoneyHandler(IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
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
        {
            
            await _outboxWriter.EnqueueAsync(new TransferFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountId,
                command.ToAccountId,
                command.Amount,
                command.Currency,
                "Account not found"
            ), ct);
             
            return;
        }

            var moneyVO = new MoneyVO(command.Amount, command.Currency);

            fromAccount.TransferTo(toAccount, moneyVO, command.TransactionId);


            await _unitOfWork.SaveChangesAsync(ct);
       
    }

}

