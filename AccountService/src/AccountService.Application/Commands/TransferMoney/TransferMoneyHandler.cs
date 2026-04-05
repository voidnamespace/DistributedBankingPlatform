using AccountService.Application.IntegrationEvents.Transactions.Transfer;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using AccountService.Domain.Entity;
using AccountService.Domain.Enums;
using AccountService.Domain.ValueObjects;
using MediatR;

namespace AccountService.Application.Commands.TransferMoney;

public class TransferMoneyHandler : IRequestHandler<TransferMoneyCommand>
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

    private async Task PublishTransferFailed(
    TransferMoneyCommand command,
    string reason,
    CancellationToken ct)
    {
        await _outboxWriter.EnqueueAsync(
            new TransferFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountNumber,
                command.ToAccountNumber,
                command.Amount,
                command.Currency,
                reason
            ),
            ct);
    }


    public async Task Handle(TransferMoneyCommand command, CancellationToken ct)
    {         
        if (command.FromAccountNumber == command.ToAccountNumber)
        {
            await PublishTransferFailed(command,
                "SameAccountTransfer",
                ct);
            return;
        }
            
        if (command.Amount <= 0)
        {
            await PublishTransferFailed(command,
                "InvalidAmount",
                ct);
            return;
        }      

        var fromAccVO = new AccountNumberVO(command.FromAccountNumber.ToString());

        var fromAccount = await _accountRepository.GetByAccountNumberAsync(fromAccVO, ct);

        if (fromAccount == null)
        {
            await PublishTransferFailed(command,
                "FromAccountNotFound",
                ct);
            return;
        }
            

        if (command.InitiatorId != fromAccount.UserId)
        {
            await PublishTransferFailed(command,
                "OwnershipMismatch",
                ct);
            return;
        }

        var toAccVo = new AccountNumberVO(command.ToAccountNumber.ToString());

        
        var toAccount = await _accountRepository.GetByAccountNumberAsync(toAccVo, ct);

        if (toAccount == null)
        {
            await PublishTransferFailed(command,
                "ToAccountNotFound",
                ct);
            return;
        }

        if (!Enum.IsDefined(typeof(Currency), command.Currency))
        {
            await PublishTransferFailed(command,
                "InvalidCurrency",
                ct);
            return;
        }

        var currency = (Currency)command.Currency;
        var moneyVO = new MoneyVO(command.Amount, currency);

        fromAccount.TransferTo(toAccount, moneyVO, command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

    }

}
