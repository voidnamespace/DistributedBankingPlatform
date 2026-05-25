using MediatR;
using UserSegmentationService.Application.IntegrationEvents.Accounts;
using UserSegmentationService.Application.Interfaces;
using UserSegmentationService.Application.Interfaces.Messaging;

namespace UserSegmentationService.Application.Commands.Accounts.RequestBackfill;

public class RequestUserAccountsBackfillHandler : IRequestHandler<RequestUserAccountsBackfillCommand>
{
    private readonly IOutboxWriter _outboxWriter;
    private readonly IUnitOfWork _unitOfWork;


    public RequestUserAccountsBackfillHandler(
        IOutboxWriter outboxWriter,
        IUnitOfWork unitOfWork)
    {
        _outboxWriter = outboxWriter;
        _unitOfWork = unitOfWork;
    }


    public async Task Handle(RequestUserAccountsBackfillCommand command,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid();

        var requestedAt = DateTime.UtcNow;

        var integrationEvent = new UserAccountsBackfillRequestedIntegrationEvent(requestId, requestedAt);

        await _outboxWriter.EnqueueAsync(integrationEvent, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

    }

}
