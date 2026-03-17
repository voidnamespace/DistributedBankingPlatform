using MediatR;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.UpdateTransactionStatus;

public class UpdateTransactionStatusHandler : IRequestHandler
{

    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;


    public UpdateTransactionStatusHandler(ITransactionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }



    public async Task Handle(UpdateTransactionStatusCommand command, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);
        if (transaction == null)
            throw new KeyNotFoundException("asd"); 
        
        if(command.Status ==)
    }



}
