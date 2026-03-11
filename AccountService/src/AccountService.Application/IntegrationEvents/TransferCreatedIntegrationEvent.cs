using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.IntegrationEvents;

public record TransferCreatedIntegrationEvent(Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency currency) : INotification;

