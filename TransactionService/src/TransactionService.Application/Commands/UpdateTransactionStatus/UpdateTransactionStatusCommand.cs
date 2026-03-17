using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.Commands.UpdateTransactionStatus;

public record UpdateTransactionStatusCommand(Guid TransactionId, TransactionStatus Status) : IRequest;

