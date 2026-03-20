using MediatR;
namespace TransactionService.Application.Commands.MarkTransactionFailed;

public record MarkTransactionFailedCommand(Guid TransactionId) : IRequest;

