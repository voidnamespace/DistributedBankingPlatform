using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.IntegrationEvents.Transactions;

public sealed record TransferFailedIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    Currency Currency) : INotification;
