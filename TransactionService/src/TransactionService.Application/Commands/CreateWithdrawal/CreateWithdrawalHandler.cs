using MediatR;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Application.Commands.CreateWithdrawal;

public class CreateWithdrawalHandler
    : IRequestHandler<CreateWithdrawalCommand, Guid>
{

    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWithdrawalHandler(ITransactionRepository transactionRepository, 
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }


    public async Task<Guid> Handle(CreateWithdrawalCommand cmd,
        CancellationToken ct)
    {
        Currency currency = (Currency)cmd.Currency;

        var money = new MoneyVO(cmd.Amount, currency);

        var fromAccountNumber = new AccountNumberVO(cmd.FromAccountNumber);

        var transaction = Transaction.CreateWithdrawal(
            cmd.InitiatorId,
            fromAccountNumber,
            money);
        
        await _transactionRepository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return transaction.TransactionId;
    }
}
