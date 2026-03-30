using AccountService.Application.IntegrationEvents.Transactions;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
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

    public async Task Handle(TransferMoneyCommand command, CancellationToken ct)
    {
        if (command.FromAccountNumber == command.ToAccountNumber)
            throw new ArgumentException("Not able to tranfer to same account");
        if (command.Amount <= 0)
            throw new ArgumentException("Amount must be greater than 0");

        var fromAccVO = new AccountNumberVO(command.FromAccountNumber.ToString());
        var toAccVo = new AccountNumberVO(command.ToAccountNumber.ToString());

        var fromAccount = await _accountRepository.GetByAccountNumberAsync(fromAccVO, ct);
        var toAccount = await _accountRepository.GetByAccountNumberAsync(toAccVo, ct);

        if (fromAccount == null || toAccount == null)
        {

            await _outboxWriter.EnqueueAsync(new TransferFailedIntegrationEvent(
                command.TransactionId,
                command.FromAccountNumber,
                command.ToAccountNumber,
                command.Amount,
                command.Currency
            ), ct);

            return;
        }

        if (!Enum.IsDefined(typeof(Currency), command.Currency))
        {
            throw new ArgumentException("Invalid currency value");
        }

        var currency = (Currency)command.Currency;
        var moneyVO = new MoneyVO(command.Amount, currency);

        fromAccount.TransferTo(toAccount, moneyVO, command.TransactionId);

        await _unitOfWork.SaveChangesAsync(ct);

    }
}
