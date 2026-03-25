using MediatR;
namespace TransactionService.Application.Queries.CheckTransferStatus;

public record CheckTransferStatusQuery(Guid TransactionId) : IRequest<string>;

