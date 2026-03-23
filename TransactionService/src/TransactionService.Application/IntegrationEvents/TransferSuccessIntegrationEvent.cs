using MediatR;
using TransactionService.Domain.Enums;

namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    Currency currency) : INotification;
