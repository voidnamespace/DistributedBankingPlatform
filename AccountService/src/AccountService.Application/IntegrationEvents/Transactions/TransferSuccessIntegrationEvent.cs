using AccountService.Domain.Enums;
using MediatR;
namespace AccountService.Application.IntegrationEvents.Transactions;

public record TransferSuccessIntegrationEvent(Guid TransactionId,
    string FromAccountNumber,
    string ToAccountNumber,
    decimal Amount,
    int Currency) : INotification;
