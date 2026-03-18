using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.IntegrationEvents.Transactions;

public record TransferCreatedIntegrationEvent(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency Currency) : INotification;

