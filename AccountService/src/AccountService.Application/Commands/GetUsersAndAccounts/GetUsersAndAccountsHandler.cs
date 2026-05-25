using AccountService.Application.IntegrationEvents.Segmentation;
using AccountService.Application.Interfaces;
using AccountService.Application.Interfaces.Messaging;
using MediatR;

namespace AccountService.Application.Commands.GetUsersAndAccounts;

public class GetUsersAndAccountsHandler : IRequestHandler<GetUsersAndAccountsCommand>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;

    public GetUsersAndAccountsHandler(
        IAccountRepository accountRepository, 
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }


    public async Task Handle(GetUsersAndAccountsCommand command,
        CancellationToken cancellationToken)
    {
        var skip = 0;
        var batchSize = 1000;
        var batchNumber = 1;

        while (true)
        {
            var accounts = await _accountRepository.GetBackfillBatchAsync(skip, batchSize, cancellationToken);

            if (accounts.Count == 0)
                break;

            var integrationEvent = new UserAccountsBackfillBatchProvidedIntegrationEvent(
                command.RequestId,
                batchNumber,
                accounts.Count < batchSize,
                DateTime.UtcNow,
                accounts.Select(x => new UserAccountBackfillItem(
                    x.UserId,
                    x.Id,
                    x.AccountNumber.Value)).ToArray());


            await _outboxWriter.EnqueueAsync(integrationEvent, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            skip += accounts.Count;
            batchNumber++;

            if (accounts.Count < batchSize)
                break;
        }
    }
}
