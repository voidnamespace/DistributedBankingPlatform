using MediatR;

namespace TransactionService.Application.Commands.MarkWithdrawalFailed;

public sealed record MarkWithdrawalFailedCommand(Guid TransactionId) : IRequest;
