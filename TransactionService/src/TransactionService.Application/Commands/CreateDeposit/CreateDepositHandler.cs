using MediatR;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.ValueObjects;

namespace TransactionService.Application.Commands.CreateDeposit;

public class CreateDepositHandler : IRequestHandler<CreateDepositCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDepositHandler(ITransactionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateDepositCommand cmd,
        CancellationToken ct)
    {
        Currency currency = (Currency)cmd.Currency;

        var money = new MoneyVO(cmd.Amount, currency);

        var toAccountNumber = new AccountNumberVO(cmd.ToAccountNumber);

        var transaction = Transaction.CreateDeposit(
            toAccountNumber,
            money);

        await _repository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return transaction.TransactionId;

    }

}
