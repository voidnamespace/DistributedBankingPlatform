using MediatR;
using TransactionService.Application.Events;
using TransactionService.Application.Interfaces;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Exceptions;
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
        var money = new MoneyVO(cmd.Amount, cmd.Currency);

        var transaction = new Transaction(
            cardId: Guid.Empty, 
            fromAccountId: cmd.FromAccountId,
            toAccountId: cmd.ToAccountId,
            money: money);

        await _repository.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return transaction.Id;
    }
}
