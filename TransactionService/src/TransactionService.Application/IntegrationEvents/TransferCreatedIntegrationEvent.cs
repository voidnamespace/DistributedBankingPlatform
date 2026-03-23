using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferCreatedIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    Currency currency) : INotification;

