using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Withdrawal;

public sealed record WithdrawalFailedIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency,
    string Reason) : INotification;
