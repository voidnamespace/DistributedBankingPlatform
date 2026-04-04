using MediatR;

namespace TransactionService.Application.Commands.MarkTransactionSuccess;

public sealed record MarkTransactionSuccessCommand(Guid TransactionId) : IRequest;

