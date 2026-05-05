using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Transfer;

public sealed record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency,
    DateTime OccurredOn) : INotification;
