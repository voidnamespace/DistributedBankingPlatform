using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions.Withdrawal;

public sealed record WithdrawalSuccessIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    decimal Amount,
    int Currency) : INotification;
