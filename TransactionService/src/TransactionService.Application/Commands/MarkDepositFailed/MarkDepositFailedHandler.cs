using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionService.Application.Commands.MarkDepositSuccess;
using TransactionService.Application.Interfaces;

namespace TransactionService.Application.Commands.MarkDepositFailed;

public class MarkDepositFailedHandler : IRequestHandler<MarkDepositFailedCommand>
{
    private readonly ITransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkDepositFailedHandler(
        ITransactionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        MarkDepositFailedCommand command,
        CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(command.TransactionId, ct);
        if (transaction == null)
            throw new KeyNotFoundException("asd");

        transaction.Fail();
        await _unitOfWork.SaveChangesAsync(ct);
    }
}
