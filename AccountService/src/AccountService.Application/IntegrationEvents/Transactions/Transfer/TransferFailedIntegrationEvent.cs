using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Transfer;

public sealed record TransferFailedIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency,
    string Reason) : INotification;
