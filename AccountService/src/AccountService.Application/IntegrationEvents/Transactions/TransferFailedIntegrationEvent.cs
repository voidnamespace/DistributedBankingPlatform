using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.IntegrationEvents.Transactions;

public sealed record TransferFailedIntegrationEvent(Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency,
    string Reason) : INotification;

