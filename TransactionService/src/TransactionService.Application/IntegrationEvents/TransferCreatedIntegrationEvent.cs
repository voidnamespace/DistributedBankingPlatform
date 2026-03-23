using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferCreatedIntegrationEvent(
    Guid TransactionId,
    string FromAccountId,
    string ToAccountId,
    decimal Amount,
    Currency currency) : INotification;

