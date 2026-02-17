namespace TransactionService.Application.Events;

public record TransferFailedEvent(
    Guid TransactionId,
    string Reason
);
