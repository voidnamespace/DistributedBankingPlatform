using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferCreatedIntegrationEvent(
    Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    Currency currency) : INotification;

