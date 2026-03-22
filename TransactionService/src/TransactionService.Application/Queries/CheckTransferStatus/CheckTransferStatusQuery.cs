using MediatR;
using TransactionService.Domain.Enums;
namespace TransactionService.Application.Queries.CheckTransferStatus;

public record CheckTransferStatusQuery(Guid TransactionId) : IRequest<TransactionStatus>;

