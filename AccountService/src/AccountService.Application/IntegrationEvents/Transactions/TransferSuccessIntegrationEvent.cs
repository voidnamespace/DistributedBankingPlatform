using AccountService.Domain.Enums;
using MediatR;

namespace AccountService.Application.IntegrationEvents.Transactions;

public record TransferSuccessIntegrationEvent(Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency) : INotification;
