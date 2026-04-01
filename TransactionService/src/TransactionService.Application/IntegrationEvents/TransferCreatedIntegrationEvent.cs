using MediatR;
namespace TransactionService.Application.IntegrationEvents;

public sealed record TransferCreatedIntegrationEvent(
    Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency,
    int Type) : INotification;
