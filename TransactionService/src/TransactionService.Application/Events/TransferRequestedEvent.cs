namespace TransactionService.Application.Events;

public record TransferRequestedEvent(
    Guid TransactionId,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string Currency
);
