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
    private readonly IEventBus _eventBus;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransferHandler(
        ITransactionRepository repository,
        IEventBus eventBus,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _eventBus = eventBus;
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

        await _eventBus.PublishAsync(
            new TransferRequestedEvent(
                transaction.Id,
                transaction.FromAccountId,
                transaction.ToAccountId,
                transaction.Money.Amount,
                transaction.Money.Currency.ToString()));

        return transaction.Id;
    }
}
