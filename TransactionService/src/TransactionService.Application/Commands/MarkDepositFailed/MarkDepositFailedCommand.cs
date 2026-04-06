using MediatR;

namespace TransactionService.Application.Commands.MarkDepositFailed;

public sealed record MarkDepositFailedCommand(Guid TransactionId) : IRequest;
