using MediatR;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkDepositSuccess;

public class MarkDepositSuccessHandler : IRequestHandler<MarkDepositSuccessCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkDepositSuccessHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        MarkDepositSuccessCommand command,
        CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);
        if (transaction == null)
            throw new KeyNotFoundException("asd");

        transaction.Complete();
        await _unitOfWork.SaveChangesAsync(ct); 
    }
}
