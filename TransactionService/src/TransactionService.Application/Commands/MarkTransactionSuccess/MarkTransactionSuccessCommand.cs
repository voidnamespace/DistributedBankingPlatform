using MediatR;
namespace TransactionService.Application.Commands.MarkTransactionSuccess;

public record MarkTransactionSuccessCommand(Guid TransactionId) : IRequest;

