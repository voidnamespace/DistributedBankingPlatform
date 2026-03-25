using MediatR;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Enums;
using TransactionService.Domain.ValueObjects;
namespace TransactionService.Application.Commands.CreateTransfer;

public class CreateTransferHandler
    : IRequestHandler<CreateTransferCommand, Guid>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransferHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateTransferCommand cmd,
        CancellationToken ct)
    {
        Currency currency = (Currency)cmd.Currency;
        var money = new MoneyVO(cmd.Amount, currency);

        var transaction = new Transaction(
            fromAccountNumber: cmd.FromAccountNumber,
            toAccountNumber: cmd.ToAccountNumber,
            money: money);

        await _repository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return transaction.TransactionId;
    }
}
