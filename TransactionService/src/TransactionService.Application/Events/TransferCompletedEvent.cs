namespace TransactionService.Application.Events;

public record TransferCompletedEvent(
    Guid TransactionId
);
