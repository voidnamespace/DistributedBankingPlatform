using MediatR;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkWithdrawalFailed;

public class MarkWithdrawalFailedHandler : IRequestHandler<MarkWithdrawalFailedCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkWithdrawalFailedHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        MarkWithdrawalFailedCommand command,
        CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);
        if (transaction == null)
            throw new KeyNotFoundException("asd");

        transaction.Fail();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
