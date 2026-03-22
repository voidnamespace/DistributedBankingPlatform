using TransactionService.Application.Interfaces;
namespace TransactionService.Application.Commands.MarkTransactionSuccess;

public class MarkTransactionSuccessHandler
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;


    public MarkTransactionSuccessHandler(ITransactionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkTransactionSuccessCommand command, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);
        if (transaction == null)
            throw new KeyNotFoundException("asd");
    
            transaction.Complete();
            await _unitOfWork.SaveChangesAsync(ct);
       
    }

}
